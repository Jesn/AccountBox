using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// 认证控制器 - 处理登录和JWT Token签发
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILoginAttemptService loginAttemptService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _loginAttemptService = loginAttemptService;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求（包含主密码）</param>
    /// <returns>登录响应（包含JWT Token和过期时间）</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        try
        {
            var result = await _authService.LoginAsync(request);
            
            // 记录登录尝试
            var isSuccessful = result.Success;
            var failureReason = isSuccessful ? null : result.Error?.ErrorCode;
            await _loginAttemptService.RecordLoginAttemptAsync(ipAddress, userAgent, isSuccessful, failureReason);

            if (result.Success)
            {
                _logger.LogInformation("User logged in successfully from IP {IP}", ipAddress);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Login failed from IP {IP}: {ErrorCode}", ipAddress, result.Error?.ErrorCode);
                return result.Error?.ErrorCode switch
                {
                    "INVALID_INPUT" => BadRequest(result),
                    "INVALID_CREDENTIALS" => Unauthorized(result),
                    _ => StatusCode(500, result)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login request processing failed from IP {IP}", ipAddress);
            return StatusCode(500, ApiResponse<LoginResponse>.Fail(
                "INTERNAL_ERROR",
                "服务器内部错误"));
        }
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <returns>登出成功消息</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        _logger.LogInformation("User logged out from IP {IP}", ipAddress);

        return Ok(new
        {
            message = "登出成功"
        });
    }


}
