namespace AccountBox.Core.Models;

/// <summary>
/// 统一 API 响应格式（符合宪法 III 要求）
/// </summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 请求是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应数据（成功时返回）
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 错误信息（失败时返回）
    /// </summary>
    public ErrorResponse? Error { get; set; }

    /// <summary>
    /// 响应时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string errorCode, string message, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorResponse
            {
                ErrorCode = errorCode,
                Message = message,
                Details = details
            },
            Timestamp = DateTime.UtcNow
        };
    }
}
