namespace AccountBox.Api.DTOs.External;

/// <summary>
/// 外部 API 更新账号信息请求 DTO
/// 所有字段可选，仅更新传入的字段（部分更新）
/// </summary>
public class ExternalUpdateAccountRequest
{
    /// <summary>
    /// 用户名（可选，传入则更新）
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 密码（可选，传入则更新，不能为空字符串）
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 标签（可选，逗号分隔）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 备注（可选）
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 扩展字段（可选，JSON 字符串）
    /// 格式: {"key1": "value1", "key2": "value2"}
    /// </summary>
    public string? Extend { get; set; }
}