using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// Account API 控制器
/// 提供账号管理的 REST API 端点（明文存储模式）
/// </summary>
[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
    }

    /// <summary>
    /// 获取分页账号列表
    /// GET /api/accounts?pageNumber=1&pageSize=10&websiteId=1&searchTerm=user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AccountResponse>>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? websiteId = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _accountService.GetPagedAsync(pageNumber, pageSize, websiteId, searchTerm);
        return Ok(ApiResponse<PagedResult<AccountResponse>>.Ok(result));
    }

    /// <summary>
    /// 根据 ID 获取账号
    /// GET /api/accounts/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> GetById(int id)
    {
        var account = await _accountService.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound(ApiResponse<AccountResponse>.Fail("NOT_FOUND", $"Account with ID {id} not found"));
        }

        return Ok(ApiResponse<AccountResponse>.Ok(account));
    }

    /// <summary>
    /// 创建账号
    /// POST /api/accounts
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> Create([FromBody] CreateAccountRequest request)
    {
        var created = await _accountService.CreateAsync(request);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ApiResponse<AccountResponse>.Ok(created));
    }

    /// <summary>
    /// 更新账号
    /// PUT /api/accounts/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AccountResponse>>> Update(
        int id,
        [FromBody] UpdateAccountRequest request)
    {
        var updated = await _accountService.UpdateAsync(id, request);
        return Ok(ApiResponse<AccountResponse>.Ok(updated));
    }

    /// <summary>
    /// 软删除账号（移入回收站）
    /// DELETE /api/accounts/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        await _accountService.SoftDeleteAsync(id);
        return Ok(ApiResponse<object>.Ok(new { message = "Account moved to recycle bin successfully" }));
    }

    /// <summary>
    /// 启用账号
    /// PUT /api/accounts/{id}/enable
    /// </summary>
    [HttpPut("{id}/enable")]
    public async Task<ActionResult<ApiResponse<object>>> Enable(int id)
    {
        try
        {
            await _accountService.EnableAccountAsync(id);
            return Ok(ApiResponse<object>.Ok(new { message = "Account enabled successfully" }));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", $"Account with ID {id} not found"));
        }
    }

    /// <summary>
    /// 禁用账号
    /// PUT /api/accounts/{id}/disable
    /// </summary>
    [HttpPut("{id}/disable")]
    public async Task<ActionResult<ApiResponse<object>>> Disable(int id)
    {
        try
        {
            await _accountService.DisableAccountAsync(id);
            return Ok(ApiResponse<object>.Ok(new { message = "Account disabled successfully" }));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", $"Account with ID {id} not found"));
        }
    }
}
