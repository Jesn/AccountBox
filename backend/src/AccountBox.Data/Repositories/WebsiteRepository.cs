using AccountBox.Core.Constants;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Data.Repositories;

/// <summary>
/// Website 仓储
/// 管理网站的 CRUD 操作、分页查询和账号统计
/// </summary>
public class WebsiteRepository : IWebsiteRepository
{
    private readonly AccountBoxDbContext _context;

    public WebsiteRepository(AccountBoxDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 获取分页网站列表（按创建时间降序）
    /// </summary>
    public async Task<(List<Website> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < PaginationConstants.DefaultPageNumber)
        {
            throw new ArgumentException($"Page number must be greater than or equal to {PaginationConstants.DefaultPageNumber}", nameof(pageNumber));
        }

        if (pageSize < PaginationConstants.MinPageSize || pageSize > PaginationConstants.MaxPageSize)
        {
            throw new ArgumentException($"Page size must be between {PaginationConstants.MinPageSize} and {PaginationConstants.MaxPageSize}", nameof(pageSize));
        }

        var query = _context.Websites.AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 获取所有网站
    /// </summary>
    public async Task<List<Website>> GetAllAsync()
    {
        return await _context.Websites
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 根据 ID 获取网站
    /// </summary>
    public async Task<Website?> GetByIdAsync(int id)
    {
        return await _context.Websites
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    /// <summary>
    /// 根据 ID 列表获取多个网站
    /// </summary>
    public async Task<List<Website>> GetByIdsAsync(List<int> ids)
    {
        return await _context.Websites
            .AsNoTracking()
            .Where(w => ids.Contains(w.Id))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 创建网站
    /// </summary>
    public async Task<Website> CreateAsync(Website website)
    {
        if (website == null)
        {
            throw new ArgumentNullException(nameof(website));
        }

        // 检查域名是否已存在
        var existingWebsite = await _context.Websites
            .FirstOrDefaultAsync(w => w.Domain == website.Domain);

        if (existingWebsite != null)
        {
            throw new InvalidOperationException($"域名 '{website.Domain}' 已存在");
        }

        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        return website;
    }

    /// <summary>
    /// 更新网站
    /// </summary>
    public async Task UpdateAsync(Website website)
    {
        if (website == null)
        {
            throw new ArgumentNullException(nameof(website));
        }

        var existing = await _context.Websites.FindAsync(website.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Website with ID {website.Id} not found");
        }

        _context.Entry(existing).CurrentValues.SetValues(website);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 删除网站
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var website = await _context.Websites.FindAsync(id);
        if (website == null)
        {
            throw new KeyNotFoundException($"Website with ID {id} not found");
        }

        _context.Websites.Remove(website);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 检查网站是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Websites.AnyAsync(w => w.Id == id);
    }

    /// <summary>
    /// 获取网站下的活跃账号数（不包括回收站）
    /// </summary>
    public async Task<int> GetActiveAccountCountAsync(int websiteId)
    {
        return await _context.Accounts
            .Where(a => a.WebsiteId == websiteId && !a.IsDeleted && a.Status == Core.Enums.AccountStatus.Active)
            .CountAsync();
    }

    /// <summary>
    /// 获取网站下的禁用账号数（不包括回收站）
    /// </summary>
    public async Task<int> GetDisabledAccountCountAsync(int websiteId)
    {
        return await _context.Accounts
            .Where(a => a.WebsiteId == websiteId && !a.IsDeleted && a.Status == Core.Enums.AccountStatus.Disabled)
            .CountAsync();
    }

    /// <summary>
    /// 获取网站下的回收站账号数
    /// </summary>
    public async Task<int> GetDeletedAccountCountAsync(int websiteId)
    {
        return await _context.Accounts
            .IgnoreQueryFilters() // 忽略全局软删除过滤器
            .Where(a => a.WebsiteId == websiteId && a.IsDeleted)
            .CountAsync();
    }

    /// <summary>
    /// 获取网站下的总账号数（包括回收站）
    /// </summary>
    public async Task<int> GetTotalAccountCountAsync(int websiteId)
    {
        return await _context.Accounts
            .IgnoreQueryFilters() // 忽略全局软删除过滤器
            .Where(a => a.WebsiteId == websiteId)
            .CountAsync();
    }
}
