using AccountBox.Core.Models;
using AccountBox.Core.Models.Search;

namespace AccountBox.Api.Services;

/// <summary>
/// 搜索业务服务接口
/// 提供全局账号搜索功能，支持搜索网站名、域名、用户名、标签、备注（明文存储模式）
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// 搜索账号
    /// </summary>
    Task<PagedResult<SearchResultItem>> SearchAsync(
        string query,
        int pageNumber,
        int pageSize);
}
