using AccountBox.Core.Constants;

namespace AccountBox.Core.Models.Search;

/// <summary>
/// 搜索请求参数
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    public required string Query { get; set; }

    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int PageNumber { get; set; } = PaginationConstants.DefaultPageNumber;

    /// <summary>
    /// 每页记录数
    /// </summary>
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}

/// <summary>
/// 搜索结果项
/// </summary>
public class SearchResultItem
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// 网站ID
    /// </summary>
    public int WebsiteId { get; set; }

    /// <summary>
    /// 网站域名
    /// </summary>
    public required string WebsiteDomain { get; set; }

    /// <summary>
    /// 网站显示名
    /// </summary>
    public string? WebsiteDisplayName { get; set; }

    /// <summary>
    /// 账号用户名
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// 账号密码（已解密）
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// 账号备注（已解密）
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 账号标签
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 匹配的字段（用于高亮显示）
    /// </summary>
    public List<string> MatchedFields { get; set; } = new();
}
