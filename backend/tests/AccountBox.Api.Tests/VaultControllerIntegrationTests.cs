using AccountBox.Api.Services;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models;
using AccountBox.Core.Models.Vault;
using AccountBox.Data.DbContext;
using AccountBox.Data.Repositories;
using AccountBox.Security.Encryption;
using AccountBox.Security.KeyDerivation;
using AccountBox.Security.VaultManager;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccountBox.Api.Tests;

/// <summary>
/// VaultController 集成测试
/// 测试完整的 initialize → unlock → lock → changePassword 流程
/// </summary>
[Collection("VaultService Tests")]
public class VaultControllerIntegrationTests : IDisposable
{
    private readonly AccountBoxDbContext _context;
    private readonly VaultService _vaultService;

    public VaultControllerIntegrationTests()
    {
        // 清除静态状态以避免测试间污染
        VaultService.ResetFailedAttemptsForTesting();

        // 使用内存数据库进行测试
        var options = new DbContextOptionsBuilder<AccountBoxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AccountBoxDbContext(options);

        // 创建依赖服务
        var argon2Service = new Argon2Service();
        var encryptionService = new AesGcmEncryptionService();
        var vaultManager = new VaultManager(argon2Service, encryptionService);
        var keySlotRepository = new KeySlotRepository(_context);

        _vaultService = new VaultService(vaultManager, keySlotRepository);
    }

    [Fact]
    public async Task Initialize_WithValidPassword_ShouldCreateKeySlotAndReturnSession()
    {
        // Arrange
        var request = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };

        // Act
        var result = await _vaultService.InitializeAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Verify KeySlot was created in database
        var keySlot = await _context.KeySlots.FirstOrDefaultAsync();
        keySlot.Should().NotBeNull();
        keySlot!.Id.Should().Be(1); // Singleton
        keySlot.EncryptedVaultKey.Should().NotBeNullOrEmpty();
        keySlot.Argon2Salt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Initialize_WhenAlreadyInitialized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };

        await _vaultService.InitializeAsync(request);

        // Act
        var act = async () => await _vaultService.InitializeAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Vault is already initialized");
    }

    [Fact]
    public async Task Unlock_WithCorrectPassword_ShouldReturnSession()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };
        await _vaultService.InitializeAsync(initRequest);

        var unlockRequest = new UnlockVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };

        // Act
        var result = await _vaultService.UnlockAsync(unlockRequest);

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Unlock_WithIncorrectPassword_ShouldThrowCryptographicException()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "CorrectPassword123!"
        };
        await _vaultService.InitializeAsync(initRequest);

        var unlockRequest = new UnlockVaultRequest
        {
            MasterPassword = "WrongPassword456!"
        };

        // Act
        var act = async () => await _vaultService.UnlockAsync(unlockRequest);

        // Assert
        await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public async Task Unlock_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new UnlockVaultRequest
        {
            MasterPassword = "SomePassword123!"
        };

        // Act
        var act = async () => await _vaultService.UnlockAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Vault is not initialized");
    }

    [Fact]
    public async Task Lock_WithValidSessionId_ShouldSucceed()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };
        var session = await _vaultService.InitializeAsync(initRequest);

        // Act
        await _vaultService.LockAsync(session.SessionId);

        // Assert - Session should be removed (verify by checking GetVaultKey returns null)
        var vaultKey = _vaultService.GetVaultKey(session.SessionId);
        vaultKey.Should().BeNull();
    }

    [Fact]
    public async Task ChangeMasterPassword_WithCorrectOldPassword_ShouldSucceed()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "OldPassword123!"
        };
        await _vaultService.InitializeAsync(initRequest);

        var changePasswordRequest = new ChangeMasterPasswordRequest
        {
            OldMasterPassword = "OldPassword123!",
            NewMasterPassword = "NewPassword456!"
        };

        // Act
        await _vaultService.ChangeMasterPasswordAsync(changePasswordRequest);

        // Assert - Should be able to unlock with new password
        var unlockRequest = new UnlockVaultRequest
        {
            MasterPassword = "NewPassword456!"
        };
        var result = await _vaultService.UnlockAsync(unlockRequest);
        result.Should().NotBeNull();

        // Old password should no longer work
        var oldUnlockRequest = new UnlockVaultRequest
        {
            MasterPassword = "OldPassword123!"
        };
        var act = async () => await _vaultService.UnlockAsync(oldUnlockRequest);
        await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public async Task ChangeMasterPassword_WithIncorrectOldPassword_ShouldThrowCryptographicException()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "CorrectPassword123!"
        };
        await _vaultService.InitializeAsync(initRequest);

        var changePasswordRequest = new ChangeMasterPasswordRequest
        {
            OldMasterPassword = "WrongOldPassword!",
            NewMasterPassword = "NewPassword456!"
        };

        // Act
        var act = async () => await _vaultService.ChangeMasterPasswordAsync(changePasswordRequest);

        // Assert
        await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public async Task GetStatus_WhenNotInitialized_ShouldReturnNotInitialized()
    {
        // Act
        var result = await _vaultService.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsInitialized.Should().BeFalse();
        result.IsUnlocked.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatus_WhenInitialized_ShouldReturnInitialized()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };
        await _vaultService.InitializeAsync(initRequest);

        // Act
        var result = await _vaultService.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public async Task GetVaultKey_WithValidSession_ShouldReturnKey()
    {
        // Arrange
        var initRequest = new InitializeVaultRequest
        {
            MasterPassword = "SecurePassword123!"
        };
        var session = await _vaultService.InitializeAsync(initRequest);

        // Act
        var vaultKey = _vaultService.GetVaultKey(session.SessionId);

        // Assert
        vaultKey.Should().NotBeNull();
        vaultKey!.Length.Should().Be(32); // 256-bit key
    }

    [Fact]
    public void GetVaultKey_WithInvalidSession_ShouldReturnNull()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid().ToString();

        // Act
        var vaultKey = _vaultService.GetVaultKey(invalidSessionId);

        // Assert
        vaultKey.Should().BeNull();
    }

    public void Dispose()
    {
        // 清理测试后的静态状态
        VaultService.ResetFailedAttemptsForTesting();

        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
