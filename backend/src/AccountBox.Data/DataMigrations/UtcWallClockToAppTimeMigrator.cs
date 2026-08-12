using AccountBox.Core.Time;
using AccountBox.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountBox.Data.DataMigrations;

/// <summary>
/// 一次性数据迁移：将历史上按 UTC 墙钟写入的时间字段，
/// 按当前应用时区偏移量转换为本地墙钟时间。
/// 通过标记表保证只执行一次，避免重复 +offset。
/// </summary>
public static class UtcWallClockToAppTimeMigrator
{
    /// <summary>
    /// 迁移标识。版本变更时改 ID 可再次执行新逻辑（慎用）。
    /// </summary>
    public const string MigrationId = "20260812_UtcWallClockToAppTime_v1";

    private const string MarkerTableName = "__AppDataMigrations";

    /// <summary>
    /// 在 schema 迁移完成后调用。
    /// </summary>
    public static void Apply(AccountBoxDbContext db, ILogger logger)
    {
        try
        {
            EnsureMarkerTable(db);

            if (IsApplied(db, MigrationId))
            {
                logger.LogInformation("数据迁移 {MigrationId} 已应用，跳过", MigrationId);
                return;
            }

            var offset = AppTime.TimeZone.GetUtcOffset(DateTime.UtcNow);
            if (offset == TimeSpan.Zero)
            {
                // 应用时区本身就是 UTC，无需改写
                MarkApplied(db, MigrationId);
                logger.LogInformation(
                    "应用时区 {TimeZone} 偏移为 0，跳过时间字段转换并写入迁移标记",
                    AppTime.TimeZoneId);
                return;
            }

            logger.LogInformation(
                "开始数据迁移 {MigrationId}：将业务时间字段按 {TimeZone} 偏移 {Offset} 从 UTC 墙钟转为本地墙钟",
                MigrationId,
                AppTime.TimeZoneId,
                FormatOffsetForLog(offset));

            var updated = ApplyOffsetToAllTimestampColumns(db, offset);

            MarkApplied(db, MigrationId);

            logger.LogInformation(
                "✓ 数据迁移 {MigrationId} 完成，累计影响 {UpdatedRows} 行（各表更新行数之和）",
                MigrationId,
                updated);
        }
        catch (Exception ex)
        {
            // 数据迁移失败不阻塞启动（schema 已就绪），但需明确记录便于重试
            logger.LogError(ex,
                "数据迁移 {MigrationId} 失败。历史 UTC 时间可能仍慢于本地时间；修复后删除 {MarkerTable} 中对应行可重试",
                MigrationId,
                MarkerTableName);
        }
    }

    private static int ApplyOffsetToAllTimestampColumns(AccountBoxDbContext db, TimeSpan offset)
    {
        var total = 0;

        // ExecuteUpdate 不会走 SaveChanges，避免 UpdateTimestamps 把 UpdatedAt 刷成“现在”
        total += db.Accounts
            .IgnoreQueryFilters()
            .ExecuteUpdate(s => s
                .SetProperty(a => a.CreatedAt, a => a.CreatedAt.Add(offset))
                .SetProperty(a => a.UpdatedAt, a => a.UpdatedAt.Add(offset))
                .SetProperty(
                    a => a.DeletedAt,
                    a => a.DeletedAt.HasValue ? a.DeletedAt.Value.Add(offset) : a.DeletedAt));

        total += db.Websites
            .ExecuteUpdate(s => s
                .SetProperty(w => w.CreatedAt, w => w.CreatedAt.Add(offset))
                .SetProperty(w => w.UpdatedAt, w => w.UpdatedAt.Add(offset)));

        total += db.ApiKeys
            .ExecuteUpdate(s => s
                .SetProperty(k => k.CreatedAt, k => k.CreatedAt.Add(offset))
                .SetProperty(k => k.UpdatedAt, k => k.UpdatedAt.Add(offset))
                .SetProperty(
                    k => k.LastUsedAt,
                    k => k.LastUsedAt.HasValue ? k.LastUsedAt.Value.Add(offset) : k.LastUsedAt));

        total += db.LoginAttempts
            .ExecuteUpdate(s => s
                .SetProperty(l => l.AttemptTime, l => l.AttemptTime.Add(offset)));

        return total;
    }

    private static void EnsureMarkerTable(AccountBoxDbContext db)
    {
        var table = QuoteIdentifier(db, MarkerTableName);
        var migrationId = QuoteIdentifier(db, "MigrationId");
        var appliedAt = QuoteIdentifier(db, "AppliedAt");

        // 标识符来自内部 QuoteIdentifier，非用户输入，安全
#pragma warning disable EF1002
        db.Database.ExecuteSqlRaw(
            $"""
             CREATE TABLE IF NOT EXISTS {table} (
                 {migrationId} varchar(128) NOT NULL PRIMARY KEY,
                 {appliedAt} varchar(64) NOT NULL
             )
             """);
#pragma warning restore EF1002

    }

    private static bool IsApplied(AccountBoxDbContext db, string migrationId)
    {
        var table = QuoteIdentifier(db, MarkerTableName);
        var idCol = QuoteIdentifier(db, "MigrationId");

        var conn = db.Database.GetDbConnection();
        var shouldClose = conn.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            conn.Open();
        }

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(1) FROM {table} WHERE {idCol} = @id";
            var p = cmd.CreateParameter();
            p.ParameterName = "@id";
            p.Value = migrationId;
            cmd.Parameters.Add(p);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                conn.Close();
            }
        }
    }

    private static void MarkApplied(AccountBoxDbContext db, string migrationId)
    {
        var table = QuoteIdentifier(db, MarkerTableName);
        var idCol = QuoteIdentifier(db, "MigrationId");
        var atCol = QuoteIdentifier(db, "AppliedAt");
        var appliedAt = AppTime.Now.ToString("O");

        var conn = db.Database.GetDbConnection();
        var shouldClose = conn.State != System.Data.ConnectionState.Open;
        if (shouldClose)
        {
            conn.Open();
        }

        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $"""
                 INSERT INTO {table} ({idCol}, {atCol})
                 VALUES (@id, @at)
                 """;
            var pId = cmd.CreateParameter();
            pId.ParameterName = "@id";
            pId.Value = migrationId;
            cmd.Parameters.Add(pId);

            var pAt = cmd.CreateParameter();
            pAt.ParameterName = "@at";
            pAt.Value = appliedAt;
            cmd.Parameters.Add(pAt);

            cmd.ExecuteNonQuery();
        }
        finally
        {
            if (shouldClose)
            {
                conn.Close();
            }
        }
    }

    /// <summary>
    /// 按数据库方言引用标识符（MySQL 用反引号，其余用双引号）。
    /// </summary>
    private static string QuoteIdentifier(AccountBoxDbContext db, string name)
    {
        var provider = db.Database.ProviderName ?? string.Empty;
        if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase) ||
            provider.Contains("MariaDb", StringComparison.OrdinalIgnoreCase))
        {
            return $"`{name.Replace("`", "``", StringComparison.Ordinal)}`";
        }

        return $"\"{name.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    private static string FormatOffsetForLog(TimeSpan offset)
    {
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset.Duration();
        return $"{sign}{abs.Hours:D2}:{abs.Minutes:D2}";
    }
}
