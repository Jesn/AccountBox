using System.ComponentModel.DataAnnotations;

namespace AccountBox.Data.Entities;

/// <summary>
/// API密钥与网站的作用域关联实体（多对多）
/// </summary>
public class ApiKeyWebsiteScope
{
    /// <summary>
    /// 关联的API密钥ID
    /// </summary>
    [Required]
    public int ApiKeyId { get; set; }

    /// <summary>
    /// 允许访问的网站ID
    /// </summary>
    [Required]
    public int WebsiteId { get; set; }

    /// <summary>
    /// 导航属性：关联的API密钥
    /// </summary>
    public ApiKey? ApiKey { get; set; }

    /// <summary>
    /// 导航属性：关联的网站
    /// </summary>
    public Website? Website { get; set; }
}
