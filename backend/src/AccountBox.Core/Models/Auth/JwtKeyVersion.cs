namespace AccountBox.Core.Models.Auth;

/// <summary>
/// JWT密钥版本 - 用于密钥轮换和多版本验证
/// </summary>
public class JwtKeyVersion
{
    /// <summary>
    /// 密钥ID（版本标识，如 "v1", "v2"）
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Base64编码的密钥内容
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 密钥创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 密钥过期时间（用于验证旧Token的过渡期）
    /// null 表示当前主密钥，永不过期
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 密钥状态
    /// </summary>
    public JwtKeyStatus Status { get; set; }
}

/// <summary>
/// JWT密钥状态
/// </summary>
public enum JwtKeyStatus
{
    /// <summary>
    /// 活跃状态 - 可用于签发和验证
    /// </summary>
    Active,

    /// <summary>
    /// 仅验证 - 只能验证旧Token，不能签发新Token（过渡期）
    /// </summary>
    VerifyOnly,

    /// <summary>
    /// 已过期 - 不再使用
    /// </summary>
    Expired,

    /// <summary>
    /// 已撤销 - 紧急撤销（安全事件）
    /// </summary>
    Revoked
}

/// <summary>
/// JWT密钥存储结构
/// </summary>
public class JwtKeyStore
{
    /// <summary>
    /// 当前主密钥ID（用于签发新Token）
    /// </summary>
    public string CurrentKeyId { get; set; } = null!;

    /// <summary>
    /// 所有密钥版本列表
    /// </summary>
    public List<JwtKeyVersion> Keys { get; set; } = new();

    /// <summary>
    /// 最后轮换时间
    /// </summary>
    public DateTime? LastRotationAt { get; set; }
}
