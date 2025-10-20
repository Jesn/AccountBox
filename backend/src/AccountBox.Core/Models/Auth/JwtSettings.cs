namespace AccountBox.Core.Models.Auth;

/// <summary>
/// JWT配置模型，从appsettings.json加载
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// JWT签名密钥（至少256位，base64编码）
    /// </summary>
    public required string SecretKey { get; set; }

    /// <summary>
    /// Token签发者
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// Token受众
    /// </summary>
    public required string Audience { get; set; }

    /// <summary>
    /// Token有效期（小时数）
    /// </summary>
    public int ExpirationHours { get; set; } = 24;

    /// <summary>
    /// 是否验证Issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 是否验证Audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 是否验证过期时间
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 是否验证签名密钥
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
}
