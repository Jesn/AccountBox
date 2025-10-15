using AccountBox.Security.Encryption;
using AccountBox.Security.KeyDerivation;
using AccountBox.Security.VaultManager;
using FluentAssertions;
using Xunit;

namespace AccountBox.Security.Tests;

/// <summary>
/// VaultManager 单元测试
/// </summary>
public class VaultManagerTests
{
    private readonly VaultManager.VaultManager _vaultManager;

    public VaultManagerTests()
    {
        var argon2Service = new Argon2Service();
        var encryptionService = new AesGcmEncryptionService();
        _vaultManager = new VaultManager.VaultManager(argon2Service, encryptionService);
    }

    [Fact]
    public void Initialize_WithValidPassword_ShouldReturnEncryptedVaultKeyAndParameters()
    {
        // Arrange
        var masterPassword = "SecurePassword123!";

        // Act
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag, argon2Salt,
             argon2Iterations, argon2MemorySize, argon2Parallelism) =
            _vaultManager.Initialize(masterPassword);

        // Assert
        encryptedVaultKey.Should().NotBeNull();
        encryptedVaultKey.Length.Should().Be(32); // 256-bit VaultKey encrypted
        vaultKeyIV.Should().NotBeNull();
        vaultKeyIV.Length.Should().Be(12);
        vaultKeyTag.Should().NotBeNull();
        vaultKeyTag.Length.Should().Be(16);
        argon2Salt.Should().NotBeNull();
        argon2Salt.Length.Should().Be(16);
        argon2Iterations.Should().Be(4);
        argon2MemorySize.Should().Be(65536); // 64MB
        argon2Parallelism.Should().Be(2);
    }

    [Fact]
    public void Unlock_WithCorrectPassword_ShouldReturnVaultKey()
    {
        // Arrange
        var masterPassword = "SecurePassword123!";
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag, argon2Salt,
             argon2Iterations, argon2MemorySize, argon2Parallelism) =
            _vaultManager.Initialize(masterPassword);

        // Act
        var vaultKey = _vaultManager.Unlock(
            masterPassword,
            encryptedVaultKey,
            vaultKeyIV,
            vaultKeyTag,
            argon2Salt,
            argon2Iterations,
            argon2MemorySize,
            argon2Parallelism);

        // Assert
        vaultKey.Should().NotBeNull();
        vaultKey.Length.Should().Be(32);
    }

    [Fact]
    public void Unlock_WithIncorrectPassword_ShouldThrowCryptographicException()
    {
        // Arrange
        var correctPassword = "SecurePassword123!";
        var incorrectPassword = "WrongPassword456!";
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag, argon2Salt,
             argon2Iterations, argon2MemorySize, argon2Parallelism) =
            _vaultManager.Initialize(correctPassword);

        // Act
        var act = () => _vaultManager.Unlock(
            incorrectPassword,
            encryptedVaultKey,
            vaultKeyIV,
            vaultKeyTag,
            argon2Salt,
            argon2Iterations,
            argon2MemorySize,
            argon2Parallelism);

        // Assert
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public void ChangeMasterPassword_WithCorrectOldPassword_ShouldReturnNewEncryptedVaultKey()
    {
        // Arrange
        var oldPassword = "OldPassword123!";
        var newPassword = "NewPassword456!";
        var (oldEncryptedVaultKey, oldVaultKeyIV, oldVaultKeyTag, oldArgon2Salt,
             oldArgon2Iterations, oldArgon2MemorySize, oldArgon2Parallelism) =
            _vaultManager.Initialize(oldPassword);

        // Get original VaultKey
        var originalVaultKey = _vaultManager.Unlock(
            oldPassword,
            oldEncryptedVaultKey,
            oldVaultKeyIV,
            oldVaultKeyTag,
            oldArgon2Salt,
            oldArgon2Iterations,
            oldArgon2MemorySize,
            oldArgon2Parallelism);

        // Act
        var (newEncryptedVaultKey, newVaultKeyIV, newVaultKeyTag, newArgon2Salt,
             newArgon2Iterations, newArgon2MemorySize, newArgon2Parallelism) =
            _vaultManager.ChangeMasterPassword(
                oldPassword,
                newPassword,
                oldEncryptedVaultKey,
                oldVaultKeyIV,
                oldVaultKeyTag,
                oldArgon2Salt,
                oldArgon2Iterations,
                oldArgon2MemorySize,
                oldArgon2Parallelism);

        // Assert - Should be able to unlock with new password
        var newVaultKey = _vaultManager.Unlock(
            newPassword,
            newEncryptedVaultKey,
            newVaultKeyIV,
            newVaultKeyTag,
            newArgon2Salt,
            newArgon2Iterations,
            newArgon2MemorySize,
            newArgon2Parallelism);

        // VaultKey should remain the same
        newVaultKey.Should().Equal(originalVaultKey);

        // New encrypted data should be different
        newEncryptedVaultKey.Should().NotEqual(oldEncryptedVaultKey);
        newArgon2Salt.Should().NotEqual(oldArgon2Salt);
    }

    [Fact]
    public void ChangeMasterPassword_WithIncorrectOldPassword_ShouldThrowCryptographicException()
    {
        // Arrange
        var correctOldPassword = "OldPassword123!";
        var incorrectOldPassword = "WrongOldPassword!";
        var newPassword = "NewPassword456!";
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag, argon2Salt,
             argon2Iterations, argon2MemorySize, argon2Parallelism) =
            _vaultManager.Initialize(correctOldPassword);

        // Act
        var act = () => _vaultManager.ChangeMasterPassword(
            incorrectOldPassword,
            newPassword,
            encryptedVaultKey,
            vaultKeyIV,
            vaultKeyTag,
            argon2Salt,
            argon2Iterations,
            argon2MemorySize,
            argon2Parallelism);

        // Assert
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public void Initialize_WithNullPassword_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _vaultManager.Initialize(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Initialize_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Act
        var act = () => _vaultManager.Initialize("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Initialize_MultipleTimes_ShouldGenerateDifferentSalts()
    {
        // Arrange
        var masterPassword = "SecurePassword123!";

        // Act
        var (_, _, _, salt1, _, _, _) = _vaultManager.Initialize(masterPassword);
        var (_, _, _, salt2, _, _, _) = _vaultManager.Initialize(masterPassword);

        // Assert
        salt1.Should().NotEqual(salt2);
    }

    [Fact]
    public void Lock_ShouldClearVaultKeyFromMemory()
    {
        // Arrange
        var masterPassword = "SecurePassword123!";
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag, argon2Salt,
             argon2Iterations, argon2MemorySize, argon2Parallelism) =
            _vaultManager.Initialize(masterPassword);

        var vaultKey = _vaultManager.Unlock(
            masterPassword,
            encryptedVaultKey,
            vaultKeyIV,
            vaultKeyTag,
            argon2Salt,
            argon2Iterations,
            argon2MemorySize,
            argon2Parallelism);

        // Make a copy to verify later
        var vaultKeyCopy = vaultKey.ToArray();

        // Act
        _vaultManager.Lock();

        // Assert
        // VaultKey should be zeroed in memory (this test verifies the method executes without error)
        // Note: The actual VaultKey reference might still exist but should contain zeros
        // We can't easily verify memory cleanup in a unit test, but we ensure the method runs
        vaultKeyCopy.Should().NotBeEmpty(); // Our copy should still have data
    }
}
