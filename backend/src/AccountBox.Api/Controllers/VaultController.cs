using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Vault;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// Vault 控制器
/// 处理应用初始化、解锁、锁定和主密码管理
/// </summary>
[ApiController]
[Route("api/vault")]
public class VaultController : ControllerBase
{
    private readonly VaultService _vaultService;
    private readonly ILogger<VaultController> _logger;

    public VaultController(
        VaultService vaultService,
        ILogger<VaultController> logger)
    {
        _vaultService = vaultService ?? throw new ArgumentNullException(nameof(vaultService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取 Vault 状态
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<VaultStatusResponse>>> GetStatus()
    {
        _logger.LogInformation("Getting vault status");
        var status = await _vaultService.GetStatusAsync();
        return Ok(ApiResponse<VaultStatusResponse>.Ok(status));
    }

    /// <summary>
    /// 初始化 Vault（首次设置主密码）
    /// </summary>
    [HttpPost("initialize")]
    public async Task<ActionResult<ApiResponse<VaultSessionResponse>>> Initialize(
        [FromBody] InitializeVaultRequest request)
    {
        _logger.LogInformation("Initializing vault");
        var session = await _vaultService.InitializeAsync(request);
        return Ok(ApiResponse<VaultSessionResponse>.Ok(session));
    }

    /// <summary>
    /// 解锁 Vault
    /// </summary>
    [HttpPost("unlock")]
    public async Task<ActionResult<ApiResponse<VaultSessionResponse>>> Unlock(
        [FromBody] UnlockVaultRequest request)
    {
        _logger.LogInformation("Unlocking vault");
        var session = await _vaultService.UnlockAsync(request);
        return Ok(ApiResponse<VaultSessionResponse>.Ok(session));
    }

    /// <summary>
    /// 锁定 Vault
    /// </summary>
    [HttpPost("lock")]
    public async Task<ActionResult<ApiResponse<object>>> Lock()
    {
        var sessionId = HttpContext.Request.Headers["X-Vault-Session"].ToString();
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest(ApiResponse<object>.Fail(
                "MISSING_SESSION",
                "X-Vault-Session header is required"));
        }

        _logger.LogInformation("Locking vault for session {SessionId}", sessionId);
        await _vaultService.LockAsync(sessionId);
        return Ok(ApiResponse<object>.Ok(new { message = "Vault locked successfully" }));
    }

    /// <summary>
    /// 修改主密码
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangeMasterPasswordRequest request)
    {
        _logger.LogInformation("Changing master password");
        await _vaultService.ChangeMasterPasswordAsync(request);
        return Ok(ApiResponse<object>.Ok(new { message = "Master password changed successfully" }));
    }
}
