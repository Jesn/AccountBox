using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.ApiKey;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// API密钥管理控制器
/// </summary>
[ApiController]
[Route("api/api-keys")]
public class ApiKeysController : ControllerBase
{
    private readonly ApiKeysManagementService _apiKeysService;

    public ApiKeysController(ApiKeysManagementService apiKeysService)
    {
        _apiKeysService = apiKeysService ?? throw new ArgumentNullException(nameof(apiKeysService));
    }

    /// <summary>
    /// 获取所有API密钥
    /// GET /api/api-keys
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ApiKeyDto>>>> GetAll()
    {
        var apiKeys = await _apiKeysService.GetAllAsync();
        return Ok(ApiResponse<List<ApiKeyDto>>.Ok(apiKeys));
    }

    /// <summary>
    /// 创建新的API密钥
    /// POST /api/api-keys
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ApiKeyDto>>> Create([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var created = await _apiKeysService.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetAll),
                null,
                ApiResponse<ApiKeyDto>.Ok(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ApiKeyDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// 删除API密钥
    /// DELETE /api/api-keys/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            await _apiKeysService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { message = "API密钥已删除" }));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", $"API密钥 {id} 不存在"));
        }
    }
}
