using AccountBox.Core.Models;
using AccountBox.Core.Models.PasswordGenerator;
using AccountBox.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// 密码生成器 API 控制器
/// 提供密码生成和强度计算的 REST API 端点
/// </summary>
[ApiController]
[Route("api/password-generator")]
public class PasswordGeneratorController : ControllerBase
{
    private readonly PasswordGeneratorService _passwordGeneratorService;

    public PasswordGeneratorController(PasswordGeneratorService passwordGeneratorService)
    {
        _passwordGeneratorService = passwordGeneratorService ?? throw new ArgumentNullException(nameof(passwordGeneratorService));
    }

    /// <summary>
    /// 生成密码
    /// POST /api/password-generator/generate
    /// </summary>
    [HttpPost("generate")]
    public ActionResult<ApiResponse<GeneratePasswordResponse>> Generate([FromBody] GeneratePasswordRequest request)
    {
        try
        {
            var result = _passwordGeneratorService.Generate(request);
            return Ok(ApiResponse<GeneratePasswordResponse>.Ok(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<GeneratePasswordResponse>.Fail(
                "INVALID_REQUEST",
                ex.Message));
        }
    }

    /// <summary>
    /// 计算密码强度
    /// POST /api/password-generator/strength
    /// </summary>
    [HttpPost("strength")]
    public ActionResult<ApiResponse<PasswordStrengthResponse>> CalculateStrength([FromBody] CalculateStrengthRequest request)
    {
        var result = _passwordGeneratorService.CalculateStrength(request.Password);
        return Ok(ApiResponse<PasswordStrengthResponse>.Ok(result));
    }
}
