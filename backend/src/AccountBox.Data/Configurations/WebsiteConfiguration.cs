using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountBox.Data.Configurations;

/// <summary>
/// Website 实体配置
/// </summary>
public class WebsiteConfiguration : IEntityTypeConfiguration<Website>
{
    public void Configure(EntityTypeBuilder<Website> builder)
    {
        builder.ToTable("Websites");

        // 主键
        builder.HasKey(w => w.Id);

        // 必填字段
        builder.Property(w => w.Domain)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.DisplayName)
            .IsRequired()
            .HasMaxLength(255);

        // 可选字段
        builder.Property(w => w.Tags)
            .HasColumnType("text"); // 使用 TEXT 类型支持 MySQL

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .IsRequired();

        // 唯一索引：Domain
        builder.HasIndex(w => w.Domain)
            .IsUnique();

        // 索引：DisplayName（用于排序和搜索）
        builder.HasIndex(w => w.DisplayName);

        // 索引：CreatedAt（默认排序）
        builder.HasIndex(w => w.CreatedAt);

        // 关系：一个网站有多个账号
        builder.HasMany(w => w.Accounts)
            .WithOne(a => a.Website)
            .HasForeignKey(a => a.WebsiteId)
            .OnDelete(DeleteBehavior.Cascade); // 删除网站时级联删除账号
    }
}
