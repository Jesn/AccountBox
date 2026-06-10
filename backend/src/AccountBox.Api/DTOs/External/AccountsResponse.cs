namespace AccountBox.Api.DTOs.External;

/// <summary>
/// 外部 API 账号列表项。
/// </summary>
public class ExternalAccountResponse
{
    /// <summary>
    /// 账号 ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 网站 ID。
    /// </summary>
    public int WebsiteId { get; set; }

    /// <summary>
    /// 用户名。
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 标签，多个标签使用逗号分隔。
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 账号状态，可能值为 Active 或 Disabled。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 扩展字段。
    /// </summary>
    public Dictionary<string, object>? ExtendedData { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// 外部 API 网站账号分页响应。
/// </summary>
public class ExternalAccountsPagedResponse
{
    /// <summary>
    /// 网站 ID。
    /// </summary>
    public int WebsiteId { get; set; }

    /// <summary>
    /// 符合筛选条件的账号总数。
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码，从 1 开始。
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 每页数量。
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数。
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否存在上一页。
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// 是否存在下一页。
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// 当前页账号列表。
    /// </summary>
    public List<ExternalAccountResponse> Accounts { get; set; } = new();
}