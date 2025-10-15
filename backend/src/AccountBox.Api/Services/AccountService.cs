using System.Text;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Account;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// Account 业务服务
/// 管理账号的 CRUD 操作、密码加密/解密、分页
/// </summary>
public class AccountService
{
    private readonly AccountRepository _accountRepository;
    private readonly WebsiteRepository _websiteRepository;
    private readonly IEncryptionService _encryptionService;

    public AccountService(
        AccountRepository accountRepository,
        WebsiteRepository websiteRepository,
        IEncryptionService encryptionService)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _websiteRepository = websiteRepository ?? throw new ArgumentNullException(nameof(websiteRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    /// <summary>
    /// 获取分页账号列表（只返回活跃账号）
    /// </summary>
    public async Task<PagedResult<AccountResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId,
        byte[] vaultKey)
    {
        if (vaultKey == null || vaultKey.Length == 0)
        {
            throw new ArgumentException("Vault key is required", nameof(vaultKey));
        }

        var (items, totalCount) = await _accountRepository.GetPagedAsync(pageNumber, pageSize, websiteId);

        var accountResponses = items.Select(account => MapToResponse(account, vaultKey)).ToList();

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
    public async Task<AccountResponse?> GetByIdAsync(int id, byte[] vaultKey)
    {
        if (vaultKey == null || vaultKey.Length == 0)
        {
            throw new ArgumentException("Vault key is required", nameof(vaultKey));
        }

        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
        {
            return null;
        }

        return MapToResponse(account, vaultKey);
    }

    /// <summary>
    /// 创建账号
    /// </summary>
    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, byte[] vaultKey)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (vaultKey == null || vaultKey.Length == 0)
        {
            throw new ArgumentException("Vault key is required", nameof(vaultKey));
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

        // 加密密码
        var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
        var (encryptedPassword, passwordIV, passwordTag) = _encryptionService.Encrypt(passwordBytes, vaultKey);

        // 加密备注（如果存在）
        byte[]? encryptedNotes = null;
        byte[]? notesIV = null;
        byte[]? notesTag = null;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var notesBytes = Encoding.UTF8.GetBytes(request.Notes);
            (encryptedNotes, notesIV, notesTag) = _encryptionService.Encrypt(notesBytes, vaultKey);
        }

        var account = new Data.Entities.Account
        {
            WebsiteId = request.WebsiteId,
            Username = request.Username.Trim(),
            PasswordEncrypted = encryptedPassword,
            PasswordIV = passwordIV,
            PasswordTag = passwordTag,
            NotesEncrypted = encryptedNotes,
            NotesIV = notesIV,
            NotesTag = notesTag,
            Tags = request.Tags?.Trim()
        };

        var created = await _accountRepository.CreateAsync(account);

        // 重新加载以包含导航属性
        var reloaded = await _accountRepository.GetByIdAsync(created.Id);
        if (reloaded == null)
        {
            throw new InvalidOperationException("Failed to reload created account");
        }

        return MapToResponse(reloaded, vaultKey);
    }

    /// <summary>
    /// 更新账号
    /// </summary>
    public async Task<AccountResponse> UpdateAsync(int id, UpdateAccountRequest request, byte[] vaultKey)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (vaultKey == null || vaultKey.Length == 0)
        {
            throw new ArgumentException("Vault key is required", nameof(vaultKey));
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

        // 加密密码
        var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
        var (encryptedPassword, passwordIV, passwordTag) = _encryptionService.Encrypt(passwordBytes, vaultKey);

        // 加密备注（如果存在）
        byte[]? encryptedNotes = null;
        byte[]? notesIV = null;
        byte[]? notesTag = null;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            var notesBytes = Encoding.UTF8.GetBytes(request.Notes);
            (encryptedNotes, notesIV, notesTag) = _encryptionService.Encrypt(notesBytes, vaultKey);
        }

        existing.Username = request.Username.Trim();
        existing.PasswordEncrypted = encryptedPassword;
        existing.PasswordIV = passwordIV;
        existing.PasswordTag = passwordTag;
        existing.NotesEncrypted = encryptedNotes;
        existing.NotesIV = notesIV;
        existing.NotesTag = notesTag;
        existing.Tags = request.Tags?.Trim();

        await _accountRepository.UpdateAsync(existing);

        // 重新加载以包含导航属性
        var reloaded = await _accountRepository.GetByIdAsync(id);
        if (reloaded == null)
        {
            throw new InvalidOperationException("Failed to reload updated account");
        }

        return MapToResponse(reloaded, vaultKey);
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
    /// 将 Account 实体映射到 AccountResponse DTO（解密密码和备注）
    /// </summary>
    private AccountResponse MapToResponse(Data.Entities.Account account, byte[] vaultKey)
    {
        // 解密密码
        var decryptedPassword = _encryptionService.Decrypt(
            account.PasswordEncrypted,
            account.PasswordIV,
            account.PasswordTag,
            vaultKey);
        var password = Encoding.UTF8.GetString(decryptedPassword);

        // 解密备注（如果存在）
        string? notes = null;
        if (account.NotesEncrypted != null && account.NotesIV != null && account.NotesTag != null)
        {
            var decryptedNotes = _encryptionService.Decrypt(
                account.NotesEncrypted,
                account.NotesIV,
                account.NotesTag,
                vaultKey);
            notes = Encoding.UTF8.GetString(decryptedNotes);
        }

        return new AccountResponse
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
            IsDeleted = account.IsDeleted,
            DeletedAt = account.DeletedAt
        };
    }
}
