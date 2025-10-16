using AccountBox.Core.Enums;

namespace AccountBox.Core.Models.ApiKey;

/// <summary>
/// API密钥DTO
/// </summary>
public class ApiKeyDto
{
    /// <summary>
    /// 密钥ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 密钥名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密钥明文（仅在创建时返回）
    /// </summary>
    public string? KeyPlaintext { get; set; }

    /// <summary>
    /// 作用域类型
    /// </summary>
    public string ScopeType { get; set; } = string.Empty;

    /// <summary>
    /// 允许访问的网站ID列表（仅当ScopeType=Specific时）
    /// </summary>
    public List<int> WebsiteIds { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}

/// <summary>
/// 创建API密钥请求
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// 密钥名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 作用域类型（"All" 或 "Specific"）
    /// </summary>
    public string ScopeType { get; set; } = "All";

    /// <summary>
    /// 允许访问的网站ID列表（仅当ScopeType=Specific时必填）
    /// </summary>
    public List<int>? WebsiteIds { get; set; }
}
