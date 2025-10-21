using AccountBox.Core.Constants;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Data.Repositories;

/// <summary>
/// 搜索仓储
/// 提供全局账号搜索功能
/// </summary>
public class SearchRepository : ISearchRepository
{
    private readonly AccountBoxDbContext _context;

    public SearchRepository(AccountBoxDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 搜索账号（支持网站名、域名、用户名、标签搜索）
    /// 注意：备注字段是加密的，需要在 Service 层解密后再过滤
    /// </summary>
    public async Task<(List<Account> Items, int TotalCount)> SearchAsync(
        string query,
        int pageNumber,
        int pageSize)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (new List<Account>(), 0);
        }

        if (pageNumber < PaginationConstants.DefaultPageNumber)
        {
            throw new ArgumentException($"Page number must be greater than or equal to {PaginationConstants.DefaultPageNumber}", nameof(pageNumber));
        }

        if (pageSize < PaginationConstants.MinPageSize || pageSize > PaginationConstants.MaxPageSize)
        {
            throw new ArgumentException($"Page size must be between {PaginationConstants.MinPageSize} and {PaginationConstants.MaxPageSize}", nameof(pageSize));
        }

        // 去除首尾空格并转小写（大小写不敏感）
        var searchTerm = $"%{query.Trim().ToLower()}%";

        // 搜索账号：匹配网站显示名、域名、用户名、标签
        // 注意：只搜索活跃账号（IsDeleted=false），全局查询过滤器会自动处理
        var queryable = _context.Accounts
            .Include(a => a.Website)
            .AsNoTracking()
            .Where(a =>
                // 搜索网站显示名
                (a.Website != null && a.Website.DisplayName != null && EF.Functions.Like(a.Website.DisplayName.ToLower(), searchTerm)) ||
                // 搜索网站域名
                (a.Website != null && EF.Functions.Like(a.Website.Domain.ToLower(), searchTerm)) ||
                // 搜索账号用户名
                EF.Functions.Like(a.Username.ToLower(), searchTerm) ||
                // 搜索账号标签
                (a.Tags != null && EF.Functions.Like(a.Tags.ToLower(), searchTerm))
                // 备注字段是加密的，无法在数据库层搜索，需要在 Service 层处理
            );

        // 获取总数
        var totalCount = await queryable.CountAsync();

        // 分页查询
        var items = await queryable
            .OrderByDescending(a => a.UpdatedAt) // 按更新时间倒序
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 获取所有账号（用于备注字段的二次过滤）
    /// 注意：此方法仅用于小数据量场景，生产环境应使用 FTS5
    /// </summary>
    public async Task<List<Account>> GetAllActiveAccountsAsync()
    {
        return await _context.Accounts
            .Include(a => a.Website)
            .AsNoTracking()
            .ToListAsync();
    }
}
