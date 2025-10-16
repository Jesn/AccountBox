using AccountBox.Api.Services;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace AccountBox.Api.Controllers;

/// <summary>
/// 搜索 API 控制器
/// 提供全局账号搜索的 REST API 端点
/// </summary>
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }

    /// <summary>
    /// 从 HttpContext.Items 获取 VaultKey（由 VaultSessionMiddleware 设置）
    /// </summary>
    private byte[] GetVaultKey()
    {
        if (!HttpContext.Items.TryGetValue("VaultKey", out var vaultKeyObj) || vaultKeyObj is not byte[] vaultKey)
        {
            throw new UnauthorizedAccessException("Vault key not found in session");
        }

        return vaultKey;
    }

    /// <summary>
    /// 搜索账号
    /// GET /api/search?query=xxx&pageNumber=1&pageSize=10
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SearchResultItem>>>> Search(
        [FromQuery] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<PagedResult<SearchResultItem>>.Fail(
                "INVALID_QUERY",
                "Search query cannot be empty"));
        }

        var vaultKey = GetVaultKey();
        var result = await _searchService.SearchAsync(query, pageNumber, pageSize, vaultKey);

        return Ok(ApiResponse<PagedResult<SearchResultItem>>.Ok(result));
    }
}
