namespace AccountBox.Core.Models.Auth;

/// <summary>
/// 登录响应DTO
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT Token字符串
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Token过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
