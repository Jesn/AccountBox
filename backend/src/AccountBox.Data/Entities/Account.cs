namespace AccountBox.Data.Entities;

/// <summary>
/// Account 实体 - 表示网站上的一个账号
/// 敏感字段（Password, Notes）以加密形式存储
/// </summary>
public class Account
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 所属网站 ID（外键）
    /// </summary>
    public int WebsiteId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 加密的密码（使用 VaultKey 加密）
    /// </summary>
    public byte[] PasswordEncrypted { get; set; } = null!;

    /// <summary>
    /// 密码加密的 IV (Nonce)
    /// </summary>
    public byte[] PasswordIV { get; set; } = null!;

    /// <summary>
    /// 密码加密的认证标签
    /// </summary>
    public byte[] PasswordTag { get; set; } = null!;

    /// <summary>
    /// 备注（明文，可为空）
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 加密的备注（使用 VaultKey 加密，可为空）
    /// </summary>
    public byte[]? NotesEncrypted { get; set; }

    /// <summary>
    /// 备注加密的 IV (Nonce)
    /// </summary>
    public byte[]? NotesIV { get; set; }

    /// <summary>
    /// 备注加密的认证标签
    /// </summary>
    public byte[]? NotesTag { get; set; }

    /// <summary>
    /// 标签（JSON 数组，用于分类和过滤）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 软删除标志
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间（软删除时记录）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 导航属性：所属网站
    /// </summary>
    public Website Website { get; set; } = null!;
}
