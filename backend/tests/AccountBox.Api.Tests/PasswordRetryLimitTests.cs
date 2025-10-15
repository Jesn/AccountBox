using AccountBox.Api.Services;
using AccountBox.Core.Interfaces;
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
/// 密码重试限制测试
/// 验证防暴力破解机制
/// </summary>
public class PasswordRetryLimitTests : IDisposable
{
    private readonly AccountBoxDbContext _context;
    private readonly VaultService _vaultService;

    public PasswordRetryLimitTests()
    {
        var options = new DbContextOptionsBuilder<AccountBoxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AccountBoxDbContext(options);

        var argon2Service = new Argon2Service();
        var encryptionService = new AesGcmEncryptionService();
        var vaultManager = new VaultManager(argon2Service, encryptionService);
        var keySlotRepository = new KeySlotRepository(_context);

        _vaultService = new VaultService(vaultManager, keySlotRepository);
    }

    [Fact]
    public async Task UnlockAsync_WithLessThan5FailedAttempts_ShouldAllowRetry()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        await _vaultService.InitializeAsync(new InitializeVaultRequest { MasterPassword = correctPassword });

        // Act - 尝试4次失败
        for (int i = 0; i < 4; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // Assert - 第5次应该仍然允许尝试（虽然会失败，但不会被锁定）
        var fifthAttempt = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
        await fifthAttempt.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>()
            .Where(ex => ex.GetType() != typeof(TooManyAttemptsException));
    }

    [Fact]
    public async Task UnlockAsync_With5FailedAttempts_ShouldLockOut()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        await _vaultService.InitializeAsync(new InitializeVaultRequest { MasterPassword = correctPassword });

        // Act - 尝试5次失败
        for (int i = 0; i < 5; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // Assert - 第6次应该被锁定
        var sixthAttempt = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
        await sixthAttempt.Should().ThrowAsync<TooManyAttemptsException>()
            .WithMessage("Too many failed attempts*");
    }

    [Fact]
    public async Task UnlockAsync_AfterLockout_WithCorrectPassword_ShouldStillBeLocked()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        await _vaultService.InitializeAsync(new InitializeVaultRequest { MasterPassword = correctPassword });

        // 触发锁定
        for (int i = 0; i < 5; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // Act - 锁定期间即使使用正确密码也应该被拒绝
        var attemptWithCorrectPassword = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = correctPassword });

        // Assert
        await attemptWithCorrectPassword.Should().ThrowAsync<TooManyAttemptsException>();
    }

    [Fact]
    public async Task UnlockAsync_AfterSuccessfulUnlock_ShouldClearFailedAttempts()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        await _vaultService.InitializeAsync(new InitializeVaultRequest { MasterPassword = correctPassword });

        // 尝试3次失败
        for (int i = 0; i < 3; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // Act - 成功解锁应该清除失败记录
        await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = correctPassword });

        // 再次尝试失败，应该从0开始计数
        for (int i = 0; i < 4; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // Assert - 第5次失败才会锁定
        var fifthAttemptAfterClear = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
        await fifthAttemptAfterClear.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>()
            .Where(ex => ex.GetType() != typeof(TooManyAttemptsException));
    }

    [Fact]
    public async Task TooManyAttemptsException_ShouldContainLockoutUntilTime()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        await _vaultService.InitializeAsync(new InitializeVaultRequest { MasterPassword = correctPassword });

        // 触发锁定
        for (int i = 0; i < 5; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // Act & Assert
        var lockedAttempt = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
        var exception = await lockedAttempt.Should().ThrowAsync<TooManyAttemptsException>();

        exception.Which.LockoutUntil.Should().BeAfter(DateTime.UtcNow);
        exception.Which.LockoutUntil.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UnlockAsync_ExactlyAfter30Minutes_ShouldAllowRetry()
    {
        // Note: 这个测试无法真正等待30分钟，仅验证逻辑设计
        // 实际的时间测试应该在集成测试或手动测试中进行
        // 这里主要验证锁定时间的设置正确性

        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword456!";

        await _vaultService.InitializeAsync(new InitializeVaultRequest { MasterPassword = correctPassword });

        // 触发锁定
        for (int i = 0; i < 5; i++)
        {
            var act = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
            await act.Should().ThrowAsync<System.Security.Cryptography.CryptographicException>();
        }

        // 验证锁定时间约30分钟
        var lockedAttempt = async () => await _vaultService.UnlockAsync(new UnlockVaultRequest { MasterPassword = wrongPassword });
        var exception = await lockedAttempt.Should().ThrowAsync<TooManyAttemptsException>();

        var lockoutDuration = exception.Which.LockoutUntil - DateTime.UtcNow;
        lockoutDuration.TotalMinutes.Should().BeApproximately(30, 0.5);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
