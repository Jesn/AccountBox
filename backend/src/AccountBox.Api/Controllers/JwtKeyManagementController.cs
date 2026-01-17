using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// JWT密钥管理控制器
/// </summary>
[ApiController]
[Route("api/jwt-keys")]
[Authorize] // 需要管理员权限
public class JwtKeyManagementController : ControllerBase
{
    private readonly IJwtKeyRotationService _keyRotationService;
    private readonly ILogger<JwtKeyManagementController> _logger;

    public JwtKeyManagementController(
        IJwtKeyRotationService keyRotationService,
        ILogger<JwtKeyManagementController> logger)
    {
        _keyRotationService = keyRotationService;
        _logger = logger;
    }

    /// <summary>
    /// 获取密钥存储信息（不返回密钥内容）
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(typeof(ApiResponse<JwtKeyStoreInfo>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<JwtKeyStoreInfo>> GetKeyInfo()
    {
        try
        {
            var keyStore = _keyRotationService.GetKeyStore();

            var info = new JwtKeyStoreInfo
            {
                CurrentKeyId = keyStore.CurrentKeyId,
                LastRotationAt = keyStore.LastRotationAt,
                TotalKeys = keyStore.Keys.Count,
                ActiveKeys = keyStore.Keys.Count(k => k.Status == JwtKeyStatus.Active),
                VerifyOnlyKeys = keyStore.Keys.Count(k => k.Status == JwtKeyStatus.VerifyOnly),
                ExpiredKeys = keyStore.Keys.Count(k => k.Status == JwtKeyStatus.Expired),
                Keys = keyStore.Keys.Select(k => new JwtKeyInfo
                {
                    Id = k.Id,
                    CreatedAt = k.CreatedAt,
                    ExpiresAt = k.ExpiresAt,
                    Status = k.Status.ToString(),
                    IsCurrent = k.Id == keyStore.CurrentKeyId
                }).ToList()
            };

            return Ok(ApiResponse<JwtKeyStoreInfo>.Ok(info));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取密钥信息失败");
            return StatusCode(500, ApiResponse<JwtKeyStoreInfo>.Fail(
                "INTERNAL_ERROR", "获取密钥信息失败"));
        }
    }

    /// <summary>
    /// 手动轮换密钥
    /// </summary>
    [HttpPost("rotate")]
    [ProducesResponseType(typeof(ApiResponse<RotateKeyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RotateKeyResponse>>> RotateKey(
        [FromBody] RotateKeyRequest? request)
    {
        try
        {
            var transitionDays = request?.TransitionPeriodDays ?? 7;

            if (transitionDays < 0 || transitionDays > 90)
            {
                return BadRequest(ApiResponse<RotateKeyResponse>.Fail(
                    "INVALID_INPUT", "过渡期天数必须在 0-90 之间"));
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            _logger.LogWarning("管理员从 {IP} 触发密钥轮换，过渡期: {Days} 天",
                ipAddress, transitionDays);

            var newKey = await _keyRotationService.RotateKeyAsync(transitionDays);

            var response = new RotateKeyResponse
            {
                NewKeyId = newKey.Id,
                CreatedAt = newKey.CreatedAt,
                TransitionPeriodDays = transitionDays,
                TransitionEndsAt = DateTime.UtcNow.AddDays(transitionDays),
                Message = $"密钥轮换成功。新密钥 {newKey.Id} 已激活，过渡期 {transitionDays} 天。"
            };

            return Ok(ApiResponse<RotateKeyResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密钥轮换失败");
            return StatusCode(500, ApiResponse<RotateKeyResponse>.Fail(
                "INTERNAL_ERROR", "密钥轮换失败"));
        }
    }

    /// <summary>
    /// 撤销指定密钥
    /// </summary>
    [HttpPost("revoke/{keyId}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> RevokeKey(string keyId)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            _logger.LogWarning("管理员从 {IP} 撤销密钥: {KeyId}", ipAddress, keyId);

            await _keyRotationService.RevokeKeyAsync(keyId);

            return Ok(ApiResponse<string>.Ok($"密钥 {keyId} 已撤销"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail("INVALID_OPERATION", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤销密钥失败");
            return StatusCode(500, ApiResponse<string>.Fail(
                "INTERNAL_ERROR", "撤销密钥失败"));
        }
    }

    /// <summary>
    /// 清理过期密钥
    /// </summary>
    [HttpPost("cleanup")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> CleanupExpiredKeys()
    {
        try
        {
            await _keyRotationService.CleanupExpiredKeysAsync();
            return Ok(ApiResponse<string>.Ok("过期密钥清理完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期密钥失败");
            return StatusCode(500, ApiResponse<string>.Fail(
                "INTERNAL_ERROR", "清理过期密钥失败"));
        }
    }

    /// <summary>
    /// 检查是否需要轮换
    /// </summary>
    [HttpGet("should-rotate")]
    [ProducesResponseType(typeof(ApiResponse<ShouldRotateResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<ShouldRotateResponse>> ShouldRotate(
        [FromQuery] int rotationDays = 30)
    {
        try
        {
            var shouldRotate = _keyRotationService.ShouldRotate(rotationDays);
            var keyStore = _keyRotationService.GetKeyStore();

            var response = new ShouldRotateResponse
            {
                ShouldRotate = shouldRotate,
                LastRotationAt = keyStore.LastRotationAt,
                RotationPolicyDays = rotationDays,
                NextRotationAt = keyStore.LastRotationAt?.AddDays(rotationDays),
                Message = shouldRotate
                    ? "建议立即轮换密钥"
                    : $"距离下次建议轮换还有 {(keyStore.LastRotationAt?.AddDays(rotationDays) - DateTime.UtcNow)?.Days ?? 0} 天"
            };

            return Ok(ApiResponse<ShouldRotateResponse>.Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查轮换状态失败");
            return StatusCode(500, ApiResponse<ShouldRotateResponse>.Fail(
                "INTERNAL_ERROR", "检查轮换状态失败"));
        }
    }
}

#region DTOs

/// <summary>
/// 密钥存储信息（不包含密钥内容）
/// </summary>
public class JwtKeyStoreInfo
{
    public string CurrentKeyId { get; set; } = null!;
    public DateTime? LastRotationAt { get; set; }
    public int TotalKeys { get; set; }
    public int ActiveKeys { get; set; }
    public int VerifyOnlyKeys { get; set; }
    public int ExpiredKeys { get; set; }
    public List<JwtKeyInfo> Keys { get; set; } = new();
}

public class JwtKeyInfo
{
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = null!;
    public bool IsCurrent { get; set; }
}

public class RotateKeyRequest
{
    /// <summary>
    /// 过渡期天数（0-90），默认7天
    /// </summary>
    public int TransitionPeriodDays { get; set; } = 7;
}

public class RotateKeyResponse
{
    public string NewKeyId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int TransitionPeriodDays { get; set; }
    public DateTime TransitionEndsAt { get; set; }
    public string Message { get; set; } = null!;
}

public class ShouldRotateResponse
{
    public bool ShouldRotate { get; set; }
    public DateTime? LastRotationAt { get; set; }
    public int RotationPolicyDays { get; set; }
    public DateTime? NextRotationAt { get; set; }
    public string Message { get; set; } = null!;
}

#endregion
