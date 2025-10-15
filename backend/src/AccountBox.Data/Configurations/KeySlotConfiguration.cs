using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountBox.Data.Configurations;

/// <summary>
/// KeySlot 实体配置
/// </summary>
public class KeySlotConfiguration : IEntityTypeConfiguration<KeySlot>
{
    public void Configure(EntityTypeBuilder<KeySlot> builder)
    {
        // 配置表名和单例约束
        builder.ToTable("KeySlots", t =>
        {
            t.HasCheckConstraint("CK_KeySlot_Singleton", "[Id] = 1");
        });

        // 主键
        builder.HasKey(k => k.Id);

        // 必填字段
        builder.Property(k => k.EncryptedVaultKey)
            .IsRequired();

        builder.Property(k => k.VaultKeyIV)
            .IsRequired();

        builder.Property(k => k.VaultKeyTag)
            .IsRequired();

        builder.Property(k => k.Argon2Salt)
            .IsRequired();

        builder.Property(k => k.Argon2Iterations)
            .IsRequired();

        builder.Property(k => k.Argon2MemorySize)
            .IsRequired();

        builder.Property(k => k.Argon2Parallelism)
            .IsRequired();

        builder.Property(k => k.CreatedAt)
            .IsRequired();

        builder.Property(k => k.UpdatedAt)
            .IsRequired();

        // 索引
        builder.HasIndex(k => k.CreatedAt);
    }
}
