namespace AccountBox.Data.Entities;

/// <summary>
/// 登录尝试记录实体，用于追踪和限制登录失败
/// </summary>
public class LoginAttempt
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 客户端IP地址（支持IPv6，最大45字符）
    /// </summary>
    public required string IPAddress { get; set; }

    /// <summary>
    /// 登录尝试时间（UTC）
    /// </summary>
    public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// 失败原因（如"密码错误"、"冷却期限制"）
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// 浏览器User-Agent
    /// </summary>
    public string? UserAgent { get; set; }
}
