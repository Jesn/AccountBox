namespace AccountBox.Core.Models.Account;

/// <summary>
/// 账号响应 DTO
/// </summary>
public class AccountResponse
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string WebsiteDomain { get; set; } = string.Empty;
    public string? WebsiteDisplayName { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 解密后的明文密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public string? Tags { get; set; }

    /// <summary>
    /// 账号状态（Active/Disabled）
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// 扩展字段（JSON 格式）
    /// </summary>
    public Dictionary<string, object>? ExtendedData { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// 创建账号请求 DTO
/// </summary>
public class CreateAccountRequest
{
    public int WebsiteId { get; set; }
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 明文密码（将被加密存储）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public string? Tags { get; set; }

    /// <summary>
    /// 扩展字段（JSON 格式）
    /// </summary>
    public Dictionary<string, object>? ExtendedData { get; set; }
}

/// <summary>
/// 更新账号请求 DTO
/// </summary>
public class UpdateAccountRequest
{
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 明文密码（将被加密存储）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public string? Tags { get; set; }

    /// <summary>
    /// 扩展字段（JSON 格式）
    /// </summary>
    public Dictionary<string, object>? ExtendedData { get; set; }
}
