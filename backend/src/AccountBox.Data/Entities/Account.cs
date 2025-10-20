using AccountBox.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AccountBox.Data.Entities;

/// <summary>
/// Account 实体 - 表示网站上的一个账号
/// 密码以明文形式存储（适用于个人自托管场景）
/// </summary>
public class Account
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 所属网站 ID（外键）
    /// </summary>
    public int WebsiteId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 密码（明文存储）
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// 备注（可为空）
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 标签（JSON 数组，用于分类和过滤）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 账号状态（Active/Disabled）
    /// </summary>
    [Required]
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    /// <summary>
    /// 扩展字段（JSON键值对，10KB限制）
    /// </summary>
    [Required]
    [MaxLength(10240, ErrorMessage = "扩展字段不能超过10KB")]
    public string ExtendedData { get; set; } = "{}";

    /// <summary>
    /// 软删除标志
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 删除时间（软删除时记录）
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 导航属性：所属网站
    /// </summary>
    public Website Website { get; set; } = null!;
}
