using System.ComponentModel.DataAnnotations;
using AccountBox.Core.Enums;

namespace AccountBox.Data.Entities;

/// <summary>
/// API密钥实体
/// </summary>
public class ApiKey
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 密钥名称（用户自定义）
    /// </summary>
    [Required(ErrorMessage = "密钥名称不能为空")]
    [MaxLength(100, ErrorMessage = "密钥名称不能超过100字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密钥明文（"sk_"前缀+32字符）
    /// </summary>
    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^sk_[A-Za-z0-9]{32}$", ErrorMessage = "密钥格式无效")]
    public string KeyPlaintext { get; set; } = string.Empty;

    /// <summary>
    /// 密钥BCrypt哈希值（用于验证）
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// 作用域类型（All/Specific）
    /// </summary>
    [Required]
    public ApiKeyScopeType ScopeType { get; set; } = ApiKeyScopeType.All;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 所属保险库ID（关联到KeySlot）
    /// </summary>
    [Required]
    public int VaultId { get; set; }

    /// <summary>
    /// 导航属性：所属保险库
    /// </summary>
    public KeySlot? Vault { get; set; }

    /// <summary>
    /// 导航属性：网站作用域列表（仅当ScopeType=Specific时有数据）
    /// </summary>
    public ICollection<ApiKeyWebsiteScope> ApiKeyWebsiteScopes { get; set; } = new List<ApiKeyWebsiteScope>();
}
