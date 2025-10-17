using System.Security.Claims;

namespace AccountBox.Core.Interfaces;

/// <summary>
/// JWT Token服务接口
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// 生成JWT Token
    /// </summary>
    /// <param name="subject">主体标识（如"vault-user"）</param>
    /// <returns>JWT Token字符串和过期时间</returns>
    (string Token, DateTime ExpiresAt) GenerateToken(string subject);

    /// <summary>
    /// 验证JWT Token
    /// </summary>
    /// <param name="token">JWT Token字符串</param>
    /// <returns>验证是否成功</returns>
    bool ValidateToken(string token);

    /// <summary>
    /// 从Token中提取Claims
    /// </summary>
    /// <param name="token">JWT Token字符串</param>
    /// <returns>ClaimsPrincipal或null（如果Token无效）</returns>
    ClaimsPrincipal? GetClaimsFromToken(string token);
}
