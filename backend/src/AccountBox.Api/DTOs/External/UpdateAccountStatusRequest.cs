namespace AccountBox.Api.DTOs.External;

/// <summary>
/// 外部 API 更新账号状态请求 DTO
/// 用于启用或禁用账号
/// </summary>
public class UpdateAccountStatusRequest
{
    /// <summary>
    /// 账号状态
    /// 可选值: "Active"(启用) 或 "Disabled"(禁用)
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
