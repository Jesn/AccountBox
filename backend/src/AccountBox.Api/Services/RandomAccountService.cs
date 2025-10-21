using AccountBox.Core.Enums;
using AccountBox.Core.Models.Account;
using AccountBox.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AccountBox.Api.Services;

/// <summary>
/// 随机账号服务
/// 提供随机选择启用账号的功能（适用于外部API调用场景，如爬虫轮询）
/// 带24小时缓存防重复功能
/// </summary>
public class RandomAccountService : IRandomAccountService
{
    private readonly AccountBoxDbContext _context;
    // 使用字典存储已分配的账号：Key = $"{apiKeyId}_{websiteId}", Value = (accountId, 过期时间)
    private static readonly Dictionary<string, (int AccountId, DateTime ExpireAt)> _accountCache = new();
    private static readonly object _cacheLock = new();

    public RandomAccountService(AccountBoxDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 从指定网站随机获取一个启用状态的账号（带24小时缓存防重复）
    /// </summary>
    /// <param name="apiKeyId">API密钥ID（用于缓存key）</param>
    /// <param name="websiteId">网站ID</param>
    /// <returns>随机选中的账号响应，如果没有可用账号则返回null</returns>
    public async Task<AccountResponse?> GetRandomEnabledAccountAsync(int apiKeyId, int websiteId)
    {
        // 清理过期缓存
        CleanExpiredCache();

        var cacheKey = $"{apiKeyId}_{websiteId}";

        // 检查缓存
        lock (_cacheLock)
        {
            if (_accountCache.TryGetValue(cacheKey, out var cached))
            {
                if (cached.ExpireAt > DateTime.UtcNow)
                {
                    // 缓存未过期，返回缓存的账号
                    var cachedAccount = _context.Accounts
                        .AsNoTracking()
                        .Include(a => a.Website)
                        .Where(a => a.Id == cached.AccountId && a.Status == AccountStatus.Active)
                        .FirstOrDefault();

                    if (cachedAccount != null)
                    {
                        return MapToResponse(cachedAccount);
                    }

                    // 如果账号已被删除或禁用，清除缓存
                    _accountCache.Remove(cacheKey);
                }
                else
                {
                    // 缓存已过期，清除
                    _accountCache.Remove(cacheKey);
                }
            }
        }

        // 获取该网站下所有启用状态的账号ID
        var allEnabledAccountIds = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.WebsiteId == websiteId && a.Status == AccountStatus.Active)
            .Select(a => a.Id)
            .ToListAsync();

        if (!allEnabledAccountIds.Any())
        {
            return null;
        }

        // 排除已被其他API Key占用且未过期的账号
        var occupiedAccountIds = new HashSet<int>();
        lock (_cacheLock)
        {
            foreach (var kvp in _accountCache)
            {
                // 只排除同网站的其他API Key占用的账号
                if (kvp.Key != cacheKey && kvp.Key.EndsWith($"_{websiteId}") && kvp.Value.ExpireAt > DateTime.UtcNow)
                {
                    occupiedAccountIds.Add(kvp.Value.AccountId);
                }
            }
        }

        // 过滤出可用的账号ID
        var availableAccountIds = allEnabledAccountIds
            .Where(id => !occupiedAccountIds.Contains(id))
            .ToList();

        // 如果没有可用账号，返回null
        if (!availableAccountIds.Any())
        {
            // 所有账号都被占用了，可以考虑返回null或者忽略占用规则
            // 这里选择返回null
            return null;
        }

        // 从可用账号中随机选择一个
        var random = new Random();
        var selectedAccountId = availableAccountIds[random.Next(availableAccountIds.Count)];

        // 查询选中的账号
        var account = await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Website)
            .FirstOrDefaultAsync(a => a.Id == selectedAccountId);

        if (account == null)
        {
            return null;
        }

        // 将选中的账号加入缓存（24小时）
        lock (_cacheLock)
        {
            _accountCache[cacheKey] = (account.Id, DateTime.UtcNow.AddHours(24));
        }

        return MapToResponse(account);
    }

    /// <summary>
    /// 清理过期的缓存条目
    /// </summary>
    private static void CleanExpiredCache()
    {
        lock (_cacheLock)
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _accountCache
                .Where(kvp => kvp.Value.ExpireAt <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _accountCache.Remove(key);
            }
        }
    }

    /// <summary>
    /// 将 Account 实体映射到 AccountResponse DTO
    /// </summary>
    private AccountResponse MapToResponse(Data.Entities.Account account)
    {
        // 解析扩展字段
        Dictionary<string, object>? extendedData = null;
        if (!string.IsNullOrWhiteSpace(account.ExtendedData) && account.ExtendedData != "{}")
        {
            try
            {
                extendedData = JsonSerializer.Deserialize<Dictionary<string, object>>(account.ExtendedData);
            }
            catch
            {
                // 如果解析失败，返回 null
                extendedData = null;
            }
        }

        return new AccountResponse
        {
            Id = account.Id,
            WebsiteId = account.WebsiteId,
            WebsiteDomain = account.Website?.Domain ?? string.Empty,
            WebsiteDisplayName = account.Website?.DisplayName,
            Username = account.Username,
            Password = account.Password, // 直接返回明文密码
            Notes = account.Notes,
            Tags = account.Tags,
            Status = account.Status.ToString(),
            ExtendedData = extendedData,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            IsDeleted = account.IsDeleted,
            DeletedAt = account.DeletedAt
        };
    }
}
