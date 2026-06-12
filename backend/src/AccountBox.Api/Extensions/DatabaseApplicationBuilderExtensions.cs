using AccountBox.Core.Models.Configuration;
using AccountBox.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Api.Extensions;

public static class DatabaseApplicationBuilderExtensions
{
    public static void UseAccountBoxDatabaseMigration(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AccountBoxDbContext>();
        var dbLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var databaseOptions = scope.ServiceProvider.GetRequiredService<DatabaseOptions>();
        var dbProvider = databaseOptions.NormalizedProvider;

        try
        {
            dbLogger.LogInformation("========================================");
            dbLogger.LogInformation("开始数据库迁移检查");
            dbLogger.LogInformation("数据库类型: {DatabaseProvider}", dbProvider.ToUpperInvariant());
            dbLogger.LogInformation("========================================");

            if (databaseOptions.IsPostgreSql || databaseOptions.IsMySql)
            {
                const int maxRetries = 30;
                var retryCount = 0;
                var connected = false;

                while (retryCount < maxRetries && !connected)
                {
                    try
                    {
                        connected = db.Database.CanConnect();
                        if (!connected)
                        {
                            retryCount++;
                            if (retryCount == 1)
                            {
                                dbLogger.LogInformation("等待数据库就绪...");
                            }
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        if (retryCount >= maxRetries)
                        {
                            dbLogger.LogError(ex, "数据库连接失败，已达到最大重试次数");
                            throw;
                        }
                        Thread.Sleep(1000);
                    }
                }

                if (!connected)
                {
                    var errorMessage = dbProvider switch
                    {
                        "postgresql" or "postgres" => "无法连接到 PostgreSQL 数据库。请确保：\n" +
                            "1. PostgreSQL 服务已启动\n" +
                            "2. 数据库已创建（通过 Docker Compose 或手动创建）\n" +
                            "3. 连接字符串配置正确（CONNECTION_STRING 环境变量）",
                        "mysql" => "无法连接到 MySQL 数据库。请确保：\n" +
                            "1. MySQL 服务已启动\n" +
                            "2. 数据库已创建（通过 Docker Compose 或手动创建）\n" +
                            "3. 连接字符串配置正确（CONNECTION_STRING 环境变量）",
                        _ => "无法连接到数据库。请检查数据库配置。"
                    };
                    dbLogger.LogError(errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                dbLogger.LogInformation("✓ 数据库连接成功");
            }
            else
            {
                dbLogger.LogInformation("SQLite 将由迁移流程创建或更新数据库文件");
            }

            EnsureSqliteDirectoryExists(databaseOptions);

            var pendingMigrations = db.Database.GetPendingMigrations().ToList();
            var appliedMigrations = db.Database.GetAppliedMigrations().ToList();

            dbLogger.LogInformation("已应用的迁移数量: {AppliedMigrationCount}", appliedMigrations.Count);
            dbLogger.LogInformation("待应用的迁移数量: {PendingMigrationCount}", pendingMigrations.Count);

            if (pendingMigrations.Any())
            {
                dbLogger.LogInformation("----------------------------------------");
                dbLogger.LogInformation("发现待应用的迁移，开始应用...");
                foreach (var migration in pendingMigrations)
                {
                    dbLogger.LogInformation("  → {Migration}", migration);
                }
                dbLogger.LogInformation("----------------------------------------");

                var startTime = DateTime.UtcNow;
                db.Database.Migrate();
                var duration = DateTime.UtcNow - startTime;
                dbLogger.LogInformation("✓ 数据库迁移成功完成（耗时: {Duration:F2} 秒）", duration.TotalSeconds);
            }
            else
            {
                dbLogger.LogInformation("✓ 数据库已是最新版本，无需迁移");
            }

            dbLogger.LogInformation("========================================");
            dbLogger.LogInformation("数据库迁移检查完成");
            dbLogger.LogInformation("========================================");
        }
        catch (Exception ex)
        {
            dbLogger.LogError("========================================");
            dbLogger.LogError(ex, "❌ 数据库迁移失败");
            dbLogger.LogError("========================================");
            dbLogger.LogError("故障排查建议：");
            dbLogger.LogError("1. 检查数据库服务是否正常运行");
            dbLogger.LogError("2. 检查连接字符串配置是否正确");
            dbLogger.LogError("3. 检查数据库用户权限是否足够");
            dbLogger.LogError("4. 查看上方详细错误信息");
            dbLogger.LogError("========================================");
            throw;
        }
    }

    private static void EnsureSqliteDirectoryExists(DatabaseOptions databaseOptions)
    {
        if (!string.IsNullOrWhiteSpace(databaseOptions.SqliteDatabasePath))
        {
            var directory = Path.GetDirectoryName(databaseOptions.SqliteDatabasePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}