namespace AccountBox.Core.Models;

/// <summary>
/// 错误响应 DTO
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// 错误代码（例如：INVALID_INPUT, UNAUTHORIZED, NOT_FOUND）
    /// </summary>
    public string ErrorCode { get; set; } = null!;

    /// <summary>
    /// 错误消息（面向用户的友好描述）
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// 错误详情（可选，用于调试）
    /// </summary>
    public object? Details { get; set; }
}
