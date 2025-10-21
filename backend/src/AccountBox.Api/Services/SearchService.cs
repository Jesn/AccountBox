using AccountBox.Core.Models;
using AccountBox.Core.Models.Search;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// 搜索业务服务
/// 提供全局账号搜索功能，支持搜索网站名、域名、用户名、标签、备注（明文存储模式）
/// </summary>
public class SearchService
{
    private readonly ISearchRepository _searchRepository;

    public SearchService(ISearchRepository searchRepository)
    {
        _searchRepository = searchRepository ?? throw new ArgumentNullException(nameof(searchRepository));
    }

    /// <summary>
    /// 搜索账号
    /// </summary>
    public async Task<PagedResult<SearchResultItem>> SearchAsync(
        string query,
        int pageNumber,
        int pageSize)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new PagedResult<SearchResultItem>
            {
                Items = new List<SearchResultItem>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // 去除首尾空格
        var searchTerm = query.Trim().ToLower();

        // 从数据库搜索（匹配网站名、域名、用户名、标签）
        var (accounts, dbTotalCount) = await _searchRepository.SearchAsync(searchTerm, pageNumber, pageSize);

        // 构建结果
        var results = new List<SearchResultItem>();

        foreach (var account in accounts)
        {
            var matchedFields = new List<string>();
            var websiteDisplayName = account.Website?.DisplayName;
            var websiteDomain = account.Website?.Domain ?? string.Empty;

            // 判断哪些字段匹配了搜索词
            if (!string.IsNullOrEmpty(websiteDisplayName) && websiteDisplayName.ToLower().Contains(searchTerm))
            {
                matchedFields.Add("WebsiteDisplayName");
            }
            if (websiteDomain.ToLower().Contains(searchTerm))
            {
                matchedFields.Add("WebsiteDomain");
            }
            if (account.Username.ToLower().Contains(searchTerm))
            {
                matchedFields.Add("Username");
            }
            if (!string.IsNullOrEmpty(account.Tags) && account.Tags.ToLower().Contains(searchTerm))
            {
                matchedFields.Add("Tags");
            }
            if (!string.IsNullOrEmpty(account.Notes) && account.Notes.ToLower().Contains(searchTerm))
            {
                matchedFields.Add("Notes");
            }

            // 只添加真正匹配的结果（包括备注匹配）
            if (matchedFields.Count > 0)
            {
                results.Add(new SearchResultItem
                {
                    AccountId = account.Id,
                    WebsiteId = account.WebsiteId,
                    WebsiteDomain = websiteDomain,
                    WebsiteDisplayName = websiteDisplayName,
                    Username = account.Username,
                    Password = account.Password, // 直接返回明文密码
                    Notes = account.Notes,
                    Tags = account.Tags,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt,
                    MatchedFields = matchedFields
                });
            }
        }

        return new PagedResult<SearchResultItem>
        {
            Items = results,
            TotalCount = dbTotalCount, // 使用数据库返回的总数（近似值）
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
