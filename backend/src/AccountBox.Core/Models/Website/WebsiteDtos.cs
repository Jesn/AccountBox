namespace AccountBox.Core.Models.Website;

/// <summary>
/// 网站响应 DTO
/// </summary>
public class WebsiteResponse
{
    public int Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 活跃账号数（不包括回收站）
    /// </summary>
    public int ActiveAccountCount { get; set; }

    /// <summary>
    /// 禁用账号数（不包括回收站）
    /// </summary>
    public int DisabledAccountCount { get; set; }

    /// <summary>
    /// 回收站账号数
    /// </summary>
    public int DeletedAccountCount { get; set; }
}

/// <summary>
/// 创建网站请求 DTO
/// </summary>
public class CreateWebsiteRequest
{
    public string Domain { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// 更新网站请求 DTO
/// </summary>
public class UpdateWebsiteRequest
{
    public string Domain { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// 账号统计响应 DTO
/// </summary>
public class AccountCountResponse
{
    public int ActiveCount { get; set; }
    public int DeletedCount { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// 网站选项响应 DTO - 用于下拉选择等场景
/// 只包含必要的字段，减少数据传输
/// </summary>
public class WebsiteOptionResponse
{
    public int Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
