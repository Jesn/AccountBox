using AccountBox.Core.Enums;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Data.Repositories;

/// <summary>
/// Account 仓储
/// 管理账号的 CRUD 操作、分页查询和软删除支持
/// </summary>
public class AccountRepository
{
    private readonly AccountBoxDbContext _context;

    public AccountRepository(AccountBoxDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 获取分页账号列表（按用户名字母序）
    /// 只返回活跃账号（不包括回收站）
    /// </summary>
    public async Task<(List<Account> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId = null,
        string? searchTerm = null,
        string? status = null)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        IQueryable<Account> query = _context.Accounts
            .AsNoTracking()
            .Include(a => a.Website); // 包含网站信息

        // 按网站过滤（可选）
        if (websiteId.HasValue)
        {
            query = query.Where(a => a.WebsiteId == websiteId.Value);
        }

        // 搜索过滤（支持用户名、标签、备注模糊搜索）
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(a =>
                a.Username.ToLower().Contains(term) ||
                (a.Tags != null && a.Tags.ToLower().Contains(term)) ||
                (a.Notes != null && a.Notes.ToLower().Contains(term))
            );
        }

        // 状态过滤
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<AccountStatus>(status, out var accountStatus))
            {
                query = query.Where(a => a.Status == accountStatus);
            }
        }

        // 全局软删除过滤器已在 DbContext 配置，这里自动过滤 IsDeleted=true 的账号
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.Username) // 按用户名字母序
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 根据 ID 获取账号
    /// 注意:包括软删除的账号,以便查看回收站中的项目详情
    /// </summary>
    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .IgnoreQueryFilters() // 包括软删除的记录
            .AsNoTracking()
            .Include(a => a.Website)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// 创建账号
    /// </summary>
    public async Task<Account> CreateAsync(Account account)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    /// <summary>
    /// 更新账号
    /// </summary>
    public async Task UpdateAsync(Account account)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        var existing = await _context.Accounts.FindAsync(account.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Account with ID {account.Id} not found");
        }

        _context.Entry(existing).CurrentValues.SetValues(account);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 软删除账号（移入回收站）
    /// </summary>
    public async Task SoftDeleteAsync(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        account.IsDeleted = true;
        account.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 恢复账号（从回收站恢复）
    /// </summary>
    public async Task RestoreAsync(int id)
    {
        // 需要忽略全局软删除过滤器才能找到已删除的账号
        var account = await _context.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        if (!account.IsDeleted)
        {
            throw new InvalidOperationException($"Account with ID {id} is not deleted");
        }

        account.IsDeleted = false;
        account.DeletedAt = null;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 永久删除账号（从数据库物理删除）
    /// </summary>
    public async Task PermanentlyDeleteAsync(int id)
    {
        // 需要忽略全局软删除过滤器才能找到已删除的账号
        var account = await _context.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 检查账号是否存在
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == id);
    }

    /// <summary>
    /// 获取回收站中的账号列表（分页）
    /// </summary>
    public async Task<(List<Account> Items, int TotalCount)> GetDeletedPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId = null)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }

        IQueryable<Account> query = _context.Accounts
            .IgnoreQueryFilters() // 忽略全局软删除过滤器
            .AsNoTracking()
            .Include(a => a.Website)
            .Where(a => a.IsDeleted);

        // 按网站过滤（可选）
        if (websiteId.HasValue)
        {
            query = query.Where(a => a.WebsiteId == websiteId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.DeletedAt) // 按删除时间降序
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// 清空回收站（永久删除所有已删除的账号）
    /// </summary>
    public async Task EmptyRecycleBinAsync(int? websiteId = null)
    {
        var query = _context.Accounts
            .IgnoreQueryFilters() // 忽略全局软删除过滤器
            .Where(a => a.IsDeleted);

        // 按网站过滤（可选）
        if (websiteId.HasValue)
        {
            query = query.Where(a => a.WebsiteId == websiteId.Value);
        }

        var accountsToDelete = await query.ToListAsync();

        _context.Accounts.RemoveRange(accountsToDelete);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 检查指定网站下是否存在指定用户名的账号
    /// </summary>
    /// <param name="websiteId">网站ID</param>
    /// <param name="username">用户名</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    public async Task<bool> UsernameExistsAsync(int websiteId, string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(username));
        }

        return await _context.Accounts
            .AsNoTracking()
            .AnyAsync(a => a.WebsiteId == websiteId && a.Username == username.Trim());
    }
}
