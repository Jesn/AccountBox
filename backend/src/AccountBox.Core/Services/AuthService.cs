using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Auth;
using Microsoft.Extensions.Logging;

namespace AccountBox.Core.Services;

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly ISecretsManager _secretsManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IJwtService jwtService,
        ISecretsManager secretsManager,
        ILogger<AuthService> logger)
    {
        _jwtService = jwtService;
        _secretsManager = secretsManager;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    public Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            // 输入验证
            if (string.IsNullOrWhiteSpace(request.MasterPassword))
            {
                return Task.FromResult(ApiResponse<LoginResponse>.Fail(
                    "INVALID_INPUT",
                    "主密码不能为空"));
            }

            // 从密钥管理器获取主密码哈希
            var masterPasswordHash = _secretsManager.GetOrGenerateMasterPasswordHash();
            if (string.IsNullOrEmpty(masterPasswordHash))
            {
                _logger.LogError("MasterPasswordHash is not available");
                return Task.FromResult(ApiResponse<LoginResponse>.Fail(
                    "INTERNAL_ERROR",
                    "Authentication system is not properly configured"));
            }

            // 验证主密码（使用 BCrypt 验证，内置恒定时间比对）
            if (!_secretsManager.VerifyMasterPassword(request.MasterPassword, masterPasswordHash))
            {
                _logger.LogWarning("登录失败：密码错误");
                return Task.FromResult(ApiResponse<LoginResponse>.Fail(
                    "INVALID_CREDENTIALS",
                    "主密码错误，请重试"));
            }

            // 密码正确，生成JWT Token
            var (token, expiresAt) = _jwtService.GenerateToken("vault-user");

            var response = new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt
            };

            _logger.LogInformation("用户登录成功");
            return Task.FromResult(ApiResponse<LoginResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录过程中发生错误");
            return Task.FromResult(ApiResponse<LoginResponse>.Fail(
                "INTERNAL_ERROR",
                "登录过程中发生错误，请稍后重试"));
        }
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    public ApiResponse<object> VerifyToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return ApiResponse<object>.Fail(
                    "MISSING_TOKEN",
                    "缺少认证令牌");
            }

            if (_jwtService.ValidateToken(token))
            {
                return ApiResponse<object>.Ok(new { valid = true });
            }

            return ApiResponse<object>.Fail(
                "INVALID_TOKEN",
                "认证令牌无效");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token 验证过程中发生错误");
            return ApiResponse<object>.Fail(
                "INTERNAL_ERROR",
                "验证失败，请稍后重试");
        }
    }


}