using System.Text.Json;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Account;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace AccountBox.Api.Services;

/// <summary>
/// Account 业务服务
/// 管理账号的 CRUD 操作（明文存储，适用于个人自托管场景）
/// </summary>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IWebsiteRepository _websiteRepository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accountRepository,
        IWebsiteRepository websiteRepository,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _websiteRepository = websiteRepository ?? throw new ArgumentNullException(nameof(websiteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 获取分页账号列表（只返回活跃账号）
    /// </summary>
    public async Task<PagedResult<AccountResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId,
        string? searchTerm = null,
        string? status = null)
    {
        var (items, totalCount) = await _accountRepository.GetPagedAsync(pageNumber, pageSize, websiteId, searchTerm, status);

        var accountResponses = items.Select(MapToResponse).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<AccountResponse>
        {
            Items = accountResponses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据 ID 获取账号
    /// </summary>
    public async Task<AccountResponse?> GetByIdAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
        {
            return null;
        }

        return MapToResponse(account);
    }

    /// <summary>
    /// 创建账号
    /// </summary>
    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(request));
        }

        // 验证网站是否存在
        if (!await _websiteRepository.ExistsAsync(request.WebsiteId))
        {
            throw new KeyNotFoundException($"Website with ID {request.WebsiteId} not found");
        }

        // 序列化扩展字段
        var extendedDataJson = request.ExtendedData != null && request.ExtendedData.Count > 0
            ? JsonSerializer.Serialize(request.ExtendedData)
            : "{}";

        var account = new Data.Entities.Account
        {
            WebsiteId = request.WebsiteId,
            Username = request.Username.Trim(),
            Password = request.Password, // 明文存储
            Notes = request.Notes?.Trim(),
            Tags = request.Tags?.Trim(),
            ExtendedData = extendedDataJson
        };

        var created = await _accountRepository.CreateAsync(account);

        // 重新加载以包含导航属性
        var reloaded = await _accountRepository.GetByIdAsync(created.Id);
        if (reloaded == null)
        {
            throw new InvalidOperationException("Failed to reload created account");
        }

        return MapToResponse(reloaded);
    }

    /// <summary>
    /// 更新账号
    /// </summary>
    public async Task<AccountResponse> UpdateAsync(int id, UpdateAccountRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username cannot be empty", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password cannot be empty", nameof(request));
        }

        var existing = await _accountRepository.GetByIdAsync(id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        // 序列化扩展字段
        var extendedDataJson = request.ExtendedData != null && request.ExtendedData.Count > 0
            ? JsonSerializer.Serialize(request.ExtendedData)
            : "{}";

        existing.Username = request.Username.Trim();
        existing.Password = request.Password; // 明文存储
        existing.Notes = request.Notes?.Trim();
        existing.Tags = request.Tags?.Trim();
        existing.ExtendedData = extendedDataJson;

        await _accountRepository.UpdateAsync(existing);

        // 重新加载以包含导航属性
        var reloaded = await _accountRepository.GetByIdAsync(id);
        if (reloaded == null)
        {
            throw new InvalidOperationException("Failed to reload updated account");
        }

        return MapToResponse(reloaded);
    }

    /// <summary>
    /// 软删除账号（移入回收站）
    /// </summary>
    public async Task SoftDeleteAsync(int id)
    {
        if (!await _accountRepository.ExistsAsync(id))
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        await _accountRepository.SoftDeleteAsync(id);
    }

    /// <summary>
    /// 启用账号
    /// </summary>
    public async Task EnableAccountAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        account.Status = Core.Enums.AccountStatus.Active;
        await _accountRepository.UpdateAsync(account);
    }

    /// <summary>
    /// 禁用账号
    /// </summary>
    public async Task DisableAccountAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException($"Account with ID {id} not found");
        }

        account.Status = Core.Enums.AccountStatus.Disabled;
        await _accountRepository.UpdateAsync(account);
    }

    /// <summary>
    /// 检查指定网站下是否存在指定用户名的账号
    /// </summary>
    /// <param name="websiteId">网站ID</param>
    /// <param name="username">用户名</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    public async Task<bool> UsernameExistsAsync(int websiteId, string username)
    {
        return await _accountRepository.UsernameExistsAsync(websiteId, username);
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
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse ExtendedData for account {AccountId}. Data: {ExtendedData}",
                    account.Id, account.ExtendedData);
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
