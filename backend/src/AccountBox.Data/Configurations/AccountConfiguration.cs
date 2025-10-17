using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountBox.Data.Configurations;

/// <summary>
/// Account 实体配置
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        // 主键
        builder.HasKey(a => a.Id);

        // 必填字段
        builder.Property(a => a.WebsiteId)
            .IsRequired();

        builder.Property(a => a.Username)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Password)
            .IsRequired()
            .HasMaxLength(1000); // 明文密码，限制长度

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // 可选字段
        builder.Property(a => a.Notes)
            .HasMaxLength(5000);

        builder.Property(a => a.Tags)
            .HasMaxLength(2000); // JSON 数组

        // 索引：WebsiteId（用于查询某网站下的所有账号）
        builder.HasIndex(a => a.WebsiteId);

        // 索引：Username（用于排序）
        builder.HasIndex(a => a.Username);

        // 索引：IsDeleted（用于软删除过滤）
        builder.HasIndex(a => a.IsDeleted);

        // 组合索引：WebsiteId + IsDeleted（用于查询某网站下的活跃/已删除账号）
        builder.HasIndex(a => new { a.WebsiteId, a.IsDeleted });

        // 索引：CreatedAt（用于排序）
        builder.HasIndex(a => a.CreatedAt);

        // 索引：DeletedAt（用于回收站查询）
        builder.HasIndex(a => a.DeletedAt);

        // 外键关系已在 WebsiteConfiguration 中配置
    }
}
