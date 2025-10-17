using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.RecycleBin;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// RecycleBin API 控制器
/// 提供回收站管理的 REST API 端点
/// </summary>
[ApiController]
[Route("api/recycle-bin")]
public class RecycleBinController : ControllerBase
{
    private readonly RecycleBinService _recycleBinService;

    public RecycleBinController(RecycleBinService recycleBinService)
    {
        _recycleBinService = recycleBinService ?? throw new ArgumentNullException(nameof(recycleBinService));
    }

    /// <summary>
    /// 获取回收站中的分页账号列表
    /// GET /api/recycle-bin?pageNumber=1&pageSize=10&websiteId=1
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<DeletedAccountResponse>>>> GetDeletedAccounts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? websiteId = null)
    {
        var result = await _recycleBinService.GetDeletedAccountsAsync(pageNumber, pageSize, websiteId);
        return Ok(ApiResponse<PagedResult<DeletedAccountResponse>>.Ok(result));
    }

    /// <summary>
    /// 恢复账号（从回收站恢复到活跃状态）
    /// POST /api/recycle-bin/{id}/restore
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<ApiResponse<object>>> RestoreAccount(int id)
    {
        await _recycleBinService.RestoreAccountAsync(id);
        return Ok(ApiResponse<object>.Ok(new { message = "Account restored successfully" }));
    }

    /// <summary>
    /// 永久删除账号（从数据库物理删除）
    /// DELETE /api/recycle-bin/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> PermanentlyDeleteAccount(int id)
    {
        await _recycleBinService.PermanentlyDeleteAccountAsync(id);
        return Ok(ApiResponse<object>.Ok(new { message = "Account permanently deleted" }));
    }

    /// <summary>
    /// 清空回收站（永久删除所有已删除的账号）
    /// DELETE /api/recycle-bin
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> EmptyRecycleBin([FromQuery] int? websiteId = null)
    {
        await _recycleBinService.EmptyRecycleBinAsync(websiteId);

        var message = websiteId.HasValue
            ? $"Recycle bin emptied for website {websiteId.Value}"
            : "Recycle bin emptied successfully";

        return Ok(ApiResponse<object>.Ok(new { message }));
    }
}
