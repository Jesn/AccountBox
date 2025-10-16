namespace AccountBox.Core.Models.RecycleBin;

/// <summary>
/// 回收站中的已删除账号响应 DTO
/// </summary>
public class DeletedAccountResponse
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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 删除时间（软删除时记录）
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// 恢复账号请求 DTO
/// </summary>
public class RestoreAccountRequest
{
    public int AccountId { get; set; }
}

/// <summary>
/// 永久删除账号请求 DTO
/// </summary>
public class PermanentlyDeleteAccountRequest
{
    public int AccountId { get; set; }
}

/// <summary>
/// 清空回收站请求 DTO
/// </summary>
public class EmptyRecycleBinRequest
{
    /// <summary>
    /// 可选：仅清空特定网站的已删除账号
    /// </summary>
    public int? WebsiteId { get; set; }
}
