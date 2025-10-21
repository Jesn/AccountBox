using AccountBox.Core.Models;
using AccountBox.Core.Models.RecycleBin;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// 回收站业务服务
/// 管理已删除账号的查看、恢复和永久删除（明文存储模式）
/// </summary>
public class RecycleBinService
{
    private readonly IAccountRepository _accountRepository;

    public RecycleBinService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    /// <summary>
    /// 获取回收站中的分页账号列表
    /// </summary>
    public async Task<PagedResult<DeletedAccountResponse>> GetDeletedAccountsAsync(
        int pageNumber,
        int pageSize,
        int? websiteId)
    {
        var (items, totalCount) = await _accountRepository.GetDeletedPagedAsync(pageNumber, pageSize, websiteId);

        var deletedAccountResponses = items.Select(MapToDeletedResponse).ToList();

        return new PagedResult<DeletedAccountResponse>
        {
            Items = deletedAccountResponses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 恢复账号（从回收站恢复到活跃状态）
    /// </summary>
    public async Task RestoreAccountAsync(int accountId)
    {
        try
        {
            await _accountRepository.RestoreAsync(accountId);
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Account with ID {accountId} not found in recycle bin");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Cannot restore account: {ex.Message}");
        }
    }

    /// <summary>
    /// 永久删除账号（从数据库物理删除）
    /// </summary>
    public async Task PermanentlyDeleteAccountAsync(int accountId)
    {
        try
        {
            await _accountRepository.PermanentlyDeleteAsync(accountId);
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Account with ID {accountId} not found");
        }
    }

    /// <summary>
    /// 清空回收站（永久删除所有已删除的账号）
    /// </summary>
    public async Task EmptyRecycleBinAsync(int? websiteId = null)
    {
        await _accountRepository.EmptyRecycleBinAsync(websiteId);
    }

    /// <summary>
    /// 将 Account 实体映射到 DeletedAccountResponse DTO
    /// </summary>
    private DeletedAccountResponse MapToDeletedResponse(Data.Entities.Account account)
    {
        return new DeletedAccountResponse
        {
            Id = account.Id,
            WebsiteId = account.WebsiteId,
            WebsiteDomain = account.Website?.Domain ?? string.Empty,
            WebsiteDisplayName = account.Website?.DisplayName,
            Username = account.Username,
            Password = account.Password, // 直接返回明文密码
            Notes = account.Notes,
            Tags = account.Tags,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            DeletedAt = account.DeletedAt
        };
    }
}
