using System.Text;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Search;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// 搜索业务服务
/// 提供全局账号搜索功能，支持搜索网站名、域名、用户名、标签、备注
/// </summary>
public class SearchService
{
    private readonly SearchRepository _searchRepository;
    private readonly IEncryptionService _encryptionService;

    public SearchService(
        SearchRepository searchRepository,
        IEncryptionService encryptionService)
    {
        _searchRepository = searchRepository ?? throw new ArgumentNullException(nameof(searchRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    /// <summary>
    /// 搜索账号
    /// </summary>
    public async Task<PagedResult<SearchResultItem>> SearchAsync(
        string query,
        int pageNumber,
        int pageSize,
        byte[] vaultKey)
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

        if (vaultKey == null || vaultKey.Length == 0)
        {
            throw new ArgumentException("Vault key is required", nameof(vaultKey));
        }

        // 去除首尾空格
        var searchTerm = query.Trim().ToLower();

        // 从数据库搜索（匹配网站名、域名、用户名、标签）
        var (accounts, dbTotalCount) = await _searchRepository.SearchAsync(searchTerm, pageNumber, pageSize);

        // 解密并构建结果
        var results = new List<SearchResultItem>();

        foreach (var account in accounts)
        {
            // 解密密码
            var decryptedPassword = _encryptionService.Decrypt(
                account.PasswordEncrypted,
                vaultKey,
                account.PasswordIV,
                account.PasswordTag);
            var password = Encoding.UTF8.GetString(decryptedPassword);

            // 解密备注（如果存在）
            string? notes = null;
            if (account.NotesEncrypted != null && account.NotesIV != null && account.NotesTag != null)
            {
                var decryptedNotes = _encryptionService.Decrypt(
                    account.NotesEncrypted,
                    vaultKey,
                    account.NotesIV,
                    account.NotesTag);
                notes = Encoding.UTF8.GetString(decryptedNotes);
            }

            // 检查备注是否匹配（二次过滤）
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
            if (!string.IsNullOrEmpty(notes) && notes.ToLower().Contains(searchTerm))
            {
                matchedFields.Add("Notes");
            }

            // 只添加真正匹配的结果（包括备注匹配）
            if (matchedFields.Count > 0 || (!string.IsNullOrEmpty(notes) && notes.ToLower().Contains(searchTerm)))
            {
                results.Add(new SearchResultItem
                {
                    AccountId = account.Id,
                    WebsiteId = account.WebsiteId,
                    WebsiteDomain = websiteDomain,
                    WebsiteDisplayName = websiteDisplayName,
                    Username = account.Username,
                    Password = password,
                    Notes = notes,
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
