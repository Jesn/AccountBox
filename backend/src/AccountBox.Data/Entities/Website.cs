namespace AccountBox.Data.Entities;

/// <summary>
/// Website 实体 - 表示一个网站或服务
/// </summary>
public class Website
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 域名（例如：github.com，用于唯一标识）
    /// </summary>
    public string Domain { get; set; } = null!;

    /// <summary>
    /// 显示名称（例如：GitHub，用于用户友好展示）
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 标签（JSON 数组，用于分类和过滤）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 导航属性：关联的账号列表
    /// </summary>
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
