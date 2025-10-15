using AccountBox.Core.Models;
using AccountBox.Core.Models.Website;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// Website 业务服务
/// 管理网站的 CRUD 操作、分页和业务验证
/// </summary>
public class WebsiteService
{
    private readonly WebsiteRepository _websiteRepository;

    public WebsiteService(WebsiteRepository websiteRepository)
    {
        _websiteRepository = websiteRepository ?? throw new ArgumentNullException(nameof(websiteRepository));
    }

    /// <summary>
    /// 获取分页网站列表
    /// </summary>
    public async Task<PagedResult<WebsiteResponse>> GetPagedAsync(int pageNumber, int pageSize)
    {
        var (items, totalCount) = await _websiteRepository.GetPagedAsync(pageNumber, pageSize);

        var websiteResponses = new List<WebsiteResponse>();

        foreach (var website in items)
        {
            var activeCount = await _websiteRepository.GetActiveAccountCountAsync(website.Id);
            var deletedCount = await _websiteRepository.GetDeletedAccountCountAsync(website.Id);

            websiteResponses.Add(new WebsiteResponse
            {
                Id = website.Id,
                Domain = website.Domain,
                DisplayName = website.DisplayName,
                Tags = website.Tags,
                CreatedAt = website.CreatedAt,
                UpdatedAt = website.UpdatedAt,
                ActiveAccountCount = activeCount,
                DeletedAccountCount = deletedCount
            });
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<WebsiteResponse>
        {
            Items = websiteResponses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据 ID 获取网站
    /// </summary>
    public async Task<WebsiteResponse?> GetByIdAsync(int id)
    {
        var website = await _websiteRepository.GetByIdAsync(id);
        if (website == null)
        {
            return null;
        }

        var activeCount = await _websiteRepository.GetActiveAccountCountAsync(website.Id);
        var deletedCount = await _websiteRepository.GetDeletedAccountCountAsync(website.Id);

        return new WebsiteResponse
        {
            Id = website.Id,
            Domain = website.Domain,
            DisplayName = website.DisplayName,
            Tags = website.Tags,
            CreatedAt = website.CreatedAt,
            UpdatedAt = website.UpdatedAt,
            ActiveAccountCount = activeCount,
            DeletedAccountCount = deletedCount
        };
    }

    /// <summary>
    /// 创建网站
    /// </summary>
    public async Task<WebsiteResponse> CreateAsync(CreateWebsiteRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Domain))
        {
            throw new ArgumentException("Domain cannot be empty", nameof(request));
        }

        var website = new Website
        {
            Domain = request.Domain.Trim(),
            DisplayName = request.DisplayName?.Trim(),
            Tags = request.Tags?.Trim()
        };

        var created = await _websiteRepository.CreateAsync(website);

        return new WebsiteResponse
        {
            Id = created.Id,
            Domain = created.Domain,
            DisplayName = created.DisplayName,
            Tags = created.Tags,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt,
            ActiveAccountCount = 0,
            DeletedAccountCount = 0
        };
    }

    /// <summary>
    /// 更新网站
    /// </summary>
    public async Task<WebsiteResponse> UpdateAsync(int id, UpdateWebsiteRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Domain))
        {
            throw new ArgumentException("Domain cannot be empty", nameof(request));
        }

        var existing = await _websiteRepository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Website with ID {id} not found");
        }

        existing.Domain = request.Domain.Trim();
        existing.DisplayName = request.DisplayName?.Trim();
        existing.Tags = request.Tags?.Trim();

        await _websiteRepository.UpdateAsync(existing);

        var activeCount = await _websiteRepository.GetActiveAccountCountAsync(existing.Id);
        var deletedCount = await _websiteRepository.GetDeletedAccountCountAsync(existing.Id);

        return new WebsiteResponse
        {
            Id = existing.Id,
            Domain = existing.Domain,
            DisplayName = existing.DisplayName,
            Tags = existing.Tags,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = existing.UpdatedAt,
            ActiveAccountCount = activeCount,
            DeletedAccountCount = deletedCount
        };
    }

    /// <summary>
    /// 删除网站
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        if (!await _websiteRepository.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"Website with ID {id} not found");
        }

        await _websiteRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 获取网站的账号统计
    /// </summary>
    public async Task<AccountCountResponse> GetAccountCountAsync(int id)
    {
        if (!await _websiteRepository.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"Website with ID {id} not found");
        }

        var activeCount = await _websiteRepository.GetActiveAccountCountAsync(id);
        var deletedCount = await _websiteRepository.GetDeletedAccountCountAsync(id);
        var totalCount = await _websiteRepository.GetTotalAccountCountAsync(id);

        return new AccountCountResponse
        {
            ActiveCount = activeCount,
            DeletedCount = deletedCount,
            TotalCount = totalCount
        };
    }
}
