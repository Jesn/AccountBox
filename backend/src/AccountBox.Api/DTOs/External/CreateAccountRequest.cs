namespace AccountBox.Api.DTOs.External;

/// <summary>
/// 外部 API 创建账号请求 DTO
/// 用于外部系统通过 API 创建账号,支持扩展字段
/// </summary>
public class ExternalCreateAccountRequest
{
    /// <summary>
    /// 网站 ID
    /// </summary>
    public int WebsiteId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 标签(可选,逗号分隔)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 备注(可选)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 扩展字段(可选,JSON 字符串)
    /// 格式:{"key1": "value1", "key2": "value2"}
    /// </summary>
    public string? Extend { get; set; }
}
