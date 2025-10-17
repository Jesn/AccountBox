using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Auth;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace AccountBox.Api.Controllers;

/// <summary>
/// 认证控制器 - 处理登录和JWT Token签发
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly AccountBoxDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IJwtService jwtService,
        AccountBoxDbContext dbContext,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求（包含主密码）</param>
    /// <returns>登录响应（包含JWT Token和过期时间）</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        try
        {
            // 从配置中读取主密码
            var masterPassword = _configuration["Authentication:MasterPassword"];
            if (string.IsNullOrEmpty(masterPassword))
            {
                _logger.LogError("MasterPassword configuration is missing");
                return StatusCode(500, new { error = new { code = "INTERNAL_ERROR", message = "Authentication system is not properly configured" } });
            }

            // 验证主密码
            if (request.MasterPassword != masterPassword)
            {
                // 记录登录失败
                await RecordLoginAttempt(ipAddress, userAgent, isSuccessful: false, failureReason: "Incorrect password");

                _logger.LogWarning("Login failed: Incorrect password from IP {IP}", ipAddress);
                return Unauthorized(new
                {
                    error = new
                    {
                        code = "PASSWORD_INCORRECT",
                        message = "主密码错误，请重试"
                    }
                });
            }

            // 密码正确，生成JWT Token
            var (token, expiresAt) = _jwtService.GenerateToken("vault-user");

            // 记录登录成功
            await RecordLoginAttempt(ipAddress, userAgent, isSuccessful: true);

            _logger.LogInformation("User logged in successfully from IP {IP}", ipAddress);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt
            });
        }
        catch (CryptographicException ex)
        {
            // 密码错误（如果将来使用加密验证）
            await RecordLoginAttempt(ipAddress, userAgent, isSuccessful: false, failureReason: "Cryptographic error");

            _logger.LogWarning(ex, "Login failed: Cryptographic error from IP {IP}", ipAddress);
            return Unauthorized(new
            {
                error = new
                {
                    code = "PASSWORD_INCORRECT",
                    message = "主密码错误，请重试"
                }
            });
        }
        catch (Exception ex)
        {
            // 其他错误
            await RecordLoginAttempt(ipAddress, userAgent, isSuccessful: false, failureReason: $"Internal error: {ex.Message}");

            _logger.LogError(ex, "Login failed: Internal error from IP {IP}", ipAddress);
            return StatusCode(500, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "登录过程中发生错误，请稍后重试"
                }
            });
        }
    }

    /// <summary>
    /// 记录登录尝试（成功或失败）
    /// </summary>
    private async Task RecordLoginAttempt(string ipAddress, string? userAgent, bool isSuccessful, string? failureReason = null)
    {
        try
        {
            var loginAttempt = new LoginAttempt
            {
                IPAddress = ipAddress,
                AttemptTime = DateTime.UtcNow,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason,
                UserAgent = userAgent
            };

            _dbContext.LoginAttempts.Add(loginAttempt);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 记录失败不应该影响登录流程
            _logger.LogError(ex, "Failed to record login attempt");
        }
    }
}
