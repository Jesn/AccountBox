namespace AccountBox.Core.Enums;

/// <summary>
/// API密钥作用域类型枚举
/// </summary>
public enum ApiKeyScopeType
{
    /// <summary>
    /// 访问所有网站
    /// </summary>
    All = 0,

    /// <summary>
    /// 仅访问指定网站（需在ApiKeyWebsiteScopes表中定义）
    /// </summary>
    Specific = 1
}
