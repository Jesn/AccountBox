using System.Text;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.RecycleBin;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// 回收站业务服务
/// 管理已删除账号的查看、恢复和永久删除
/// </summary>
public class RecycleBinService
{
    private readonly AccountRepository _accountRepository;
    private readonly IEncryptionService _encryptionService;

    public RecycleBinService(
        AccountRepository accountRepository,
        IEncryptionService encryptionService)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    /// <summary>
    /// 获取回收站中的分页账号列表
    /// </summary>
    public async Task<PagedResult<DeletedAccountResponse>> GetDeletedAccountsAsync(
        int pageNumber,
        int pageSize,
        int? websiteId,
        byte[] vaultKey)
    {
        if (vaultKey == null || vaultKey.Length == 0)
        {
            throw new ArgumentException("Vault key is required", nameof(vaultKey));
        }

        var (items, totalCount) = await _accountRepository.GetDeletedPagedAsync(pageNumber, pageSize, websiteId);

        var deletedAccountResponses = items.Select(account => MapToDeletedResponse(account, vaultKey)).ToList();

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
    /// 将 Account 实体映射到 DeletedAccountResponse DTO（解密密码和备注）
    /// </summary>
    private DeletedAccountResponse MapToDeletedResponse(Data.Entities.Account account, byte[] vaultKey)
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

        return new DeletedAccountResponse
        {
            Id = account.Id,
            WebsiteId = account.WebsiteId,
            WebsiteDomain = account.Website?.Domain ?? string.Empty,
            WebsiteDisplayName = account.Website?.DisplayName,
            Username = account.Username,
            Password = password,
            Notes = notes,
            Tags = account.Tags,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            DeletedAt = account.DeletedAt
        };
    }
}
