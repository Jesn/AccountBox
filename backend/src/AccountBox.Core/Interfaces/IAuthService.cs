using AccountBox.Core.Models;
using AccountBox.Core.Models.Auth;

namespace AccountBox.Core.Interfaces;

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>登录结果</returns>
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// 验证 Token
    /// </summary>
    /// <param name="token">JWT Token</param>
    /// <returns>验证结果</returns>
    ApiResponse<object> VerifyToken(string token);
}