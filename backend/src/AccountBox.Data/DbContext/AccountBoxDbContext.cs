using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
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
