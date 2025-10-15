namespace AccountBox.Data.Entities;

/// <summary>
/// KeySlot 实体 - 存储加密的 VaultKey 和 Argon2 参数
/// 设计为单例模式（数据库中只有一条记录，Id = 1）
/// </summary>
public class KeySlot
{
    /// <summary>
    /// 主键，固定为 1（单例）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 加密的 VaultKey（使用 KEK 加密）
    /// </summary>
    public byte[] EncryptedVaultKey { get; set; } = null!;

    /// <summary>
    /// VaultKey 加密的 IV (Nonce)
    /// </summary>
    public byte[] VaultKeyIV { get; set; } = null!;

    /// <summary>
    /// VaultKey 加密的认证标签
    /// </summary>
    public byte[] VaultKeyTag { get; set; } = null!;

    /// <summary>
    /// Argon2 盐值
    /// </summary>
    public byte[] Argon2Salt { get; set; } = null!;

    /// <summary>
    /// Argon2 迭代次数
    /// </summary>
    public int Argon2Iterations { get; set; }

    /// <summary>
    /// Argon2 内存大小（KB）
    /// </summary>
    public int Argon2MemorySize { get; set; }

    /// <summary>
    /// Argon2 并行度（线程数）
    /// </summary>
    public int Argon2Parallelism { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间（用于修改主密码场景）
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
