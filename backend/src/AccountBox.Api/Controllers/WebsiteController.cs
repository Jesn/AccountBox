using AccountBox.Api.Services;
using AccountBox.Core.Exceptions;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Website;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// Website API 控制器
/// 提供网站管理的 REST API 端点
/// </summary>
[ApiController]
[Route("api/websites")]
public class WebsiteController : ControllerBase
{
    private readonly WebsiteService _websiteService;

    public WebsiteController(WebsiteService websiteService)
    {
        _websiteService = websiteService ?? throw new ArgumentNullException(nameof(websiteService));
    }

    /// <summary>
    /// 获取分页网站列表
    /// GET /api/websites?pageNumber=1&pageSize=10
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<WebsiteResponse>>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _websiteService.GetPagedAsync(pageNumber, pageSize);
        return Ok(ApiResponse<PagedResult<WebsiteResponse>>.Ok(result));
    }

    /// <summary>
    /// 根据 ID 获取网站
    /// GET /api/websites/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<WebsiteResponse>>> GetById(int id)
    {
        var website = await _websiteService.GetByIdAsync(id);
        if (website == null)
        {
            return NotFound(ApiResponse<WebsiteResponse>.Fail("NOT_FOUND", $"Website with ID {id} not found"));
        }

        return Ok(ApiResponse<WebsiteResponse>.Ok(website));
    }

    /// <summary>
    /// 创建网站
    /// POST /api/websites
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<WebsiteResponse>>> Create([FromBody] CreateWebsiteRequest request)
    {
        var created = await _websiteService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ApiResponse<WebsiteResponse>.Ok(created));
    }

    /// <summary>
    /// 更新网站
    /// PUT /api/websites/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<WebsiteResponse>>> Update(
        int id,
        [FromBody] UpdateWebsiteRequest request)
    {
        var updated = await _websiteService.UpdateAsync(id, request);
        return Ok(ApiResponse<WebsiteResponse>.Ok(updated));
    }

    /// <summary>
    /// 删除网站（级联删除保护）
    /// DELETE /api/websites/{id}?confirmed=false
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        int id,
        [FromQuery] bool confirmed = false)
    {
        try
        {
            await _websiteService.DeleteAsync(id, confirmed);
            return Ok(ApiResponse<object>.Ok(new { message = "Website deleted successfully" }));
        }
        catch (ActiveAccountsExistException ex)
        {
            // 409 Conflict: 网站下还有活跃账号
            return Conflict(ApiResponse<object>.Fail(
                "ACTIVE_ACCOUNTS_EXIST",
                ex.Message,
                new
                {
                    websiteId = ex.WebsiteId,
                    activeAccountCount = ex.ActiveAccountCount
                }));
        }
        catch (ConfirmationRequiredException ex)
        {
            // 409 Conflict: 需要用户确认删除（回收站中有账号）
            return Conflict(ApiResponse<object>.Fail(
                "CONFIRMATION_REQUIRED",
                ex.Message,
                new
                {
                    websiteId = ex.WebsiteId,
                    deletedAccountCount = ex.DeletedAccountCount
                }));
        }
    }

    /// <summary>
    /// 获取网站的账号统计
    /// GET /api/websites/{id}/accounts/count
    /// </summary>
    [HttpGet("{id}/accounts/count")]
    public async Task<ActionResult<ApiResponse<AccountCountResponse>>> GetAccountCount(int id)
    {
        var count = await _websiteService.GetAccountCountAsync(id);
        return Ok(ApiResponse<AccountCountResponse>.Ok(count));
    }
}
