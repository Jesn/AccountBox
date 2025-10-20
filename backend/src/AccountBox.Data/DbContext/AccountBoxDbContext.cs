using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccountBox.Data.DbContext;

/// <summary>
/// AccountBox 数据库上下文
/// </summary>
public class AccountBoxDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AccountBoxDbContext(DbContextOptions<AccountBoxDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Website 表
    /// </summary>
    public DbSet<Website> Websites { get; set; } = null!;

    /// <summary>
    /// Account 表
    /// </summary>
    public DbSet<Account> Accounts { get; set; } = null!;

    /// <summary>
    /// ApiKey 表
    /// </summary>
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;

    /// <summary>
    /// ApiKeyWebsiteScope 表
    /// </summary>
    public DbSet<ApiKeyWebsiteScope> ApiKeyWebsiteScopes { get; set; } = null!;

    /// <summary>
    /// LoginAttempt 表
    /// </summary>
    public DbSet<LoginAttempt> LoginAttempts { get; set; } = null!;

    /// <summary>
    /// 配置数据库连接和警告处理
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // 抑制待处理模型变更警告（在迁移时会自动处理）
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用实体配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountBoxDbContext).Assembly);

        // 全局查询过滤器：默认不查询软删除的账号
        modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);

        // ApiKey 配置
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.KeyPlaintext).IsUnique();
            entity.Property(e => e.ScopeType).HasDefaultValue(Core.Enums.ApiKeyScopeType.All);

            // 根据数据库类型设置不同的默认值
            // MySQL 的 datetime 类型不支持 CURRENT_TIMESTAMP，由应用层处理
            // PostgreSQL 和 SQLite 可以使用 CURRENT_TIMESTAMP
            if (Database.ProviderName != "Pomelo.EntityFrameworkCore.MySql")
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        });

        // ApiKeyWebsiteScope 配置（多对多关联）
        modelBuilder.Entity<ApiKeyWebsiteScope>(entity =>
        {
            entity.HasKey(e => new { e.ApiKeyId, e.WebsiteId });
            entity.HasIndex(e => e.WebsiteId);

            entity.HasOne(e => e.ApiKey)
                .WithMany(a => a.ApiKeyWebsiteScopes)
                .HasForeignKey(e => e.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Website)
                .WithMany()
                .HasForeignKey(e => e.WebsiteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LoginAttempt 配置
        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IPAddress).HasMaxLength(45).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(200);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            // 索引：用于快速查询某IP最近的失败记录
            entity.HasIndex(e => new { e.IPAddress, e.AttemptTime })
                .HasDatabaseName("IX_LoginAttempts_IPAddress_AttemptTime");

            // 索引：用于清理旧记录
            entity.HasIndex(e => e.AttemptTime)
                .HasDatabaseName("IX_LoginAttempts_AttemptTime");
        });
    }

    /// <summary>
    /// 保存更改前自动更新时间戳
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// 保存更改前自动更新时间戳（异步）
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 更新 CreatedAt 和 UpdatedAt 时间戳
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            // 尝试设置 CreatedAt（仅对新增实体）
            if (entry.State == EntityState.Added)
            {
                try
                {
                    var createdAtProperty = entry.Property("CreatedAt");
                    if (createdAtProperty != null &&
                        (createdAtProperty.CurrentValue == null ||
                         (createdAtProperty.CurrentValue is DateTime dt && dt == default)))
                    {
                        createdAtProperty.CurrentValue = now;
                    }
                }
                catch (InvalidOperationException)
                {
                    // 实体没有 CreatedAt 属性，跳过
                }
            }

            // 尝试设置 UpdatedAt
            try
            {
                var updatedAtProperty = entry.Property("UpdatedAt");
                if (updatedAtProperty != null)
                {
                    updatedAtProperty.CurrentValue = now;
                }
            }
            catch (InvalidOperationException)
            {
                // 实体没有 UpdatedAt 属性，跳过
            }
        }
    }
}
