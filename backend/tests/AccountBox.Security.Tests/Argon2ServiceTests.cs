using AccountBox.Security.KeyDerivation;
using FluentAssertions;
using Xunit;

namespace AccountBox.Security.Tests;

/// <summary>
/// Argon2Service 单元测试
/// </summary>
public class Argon2ServiceTests
{
    private readonly Argon2Service _service;

    public Argon2ServiceTests()
    {
        _service = new Argon2Service();
    }

    [Fact]
    public void DeriveKey_WithValidPassword_ShouldReturnCorrectLength()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = new byte[16];
        new Random().NextBytes(salt);
        var iterations = 4;
        var memorySizeKB = 65536; // 64MB
        var parallelism = 2;

        // Act
        var key = _service.DeriveKey(password, salt, iterations, memorySizeKB, parallelism);

        // Assert
        key.Should().NotBeNull();
        key.Length.Should().Be(32); // 256 bits
    }

    [Fact]
    public void DeriveKey_WithSameInputs_ShouldReturnSameKey()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt = new byte[16];
        new Random().NextBytes(salt);
        var iterations = 4;
        var memorySizeKB = 65536;
        var parallelism = 2;

        // Act
        var key1 = _service.DeriveKey(password, salt, iterations, memorySizeKB, parallelism);
        var key2 = _service.DeriveKey(password, salt, iterations, memorySizeKB, parallelism);

        // Assert
        key1.Should().Equal(key2);
    }

    [Fact]
    public void DeriveKey_WithDifferentPasswords_ShouldReturnDifferentKeys()
    {
        // Arrange
        var password1 = "TestPassword123!";
        var password2 = "DifferentPassword456!";
        var salt = new byte[16];
        new Random().NextBytes(salt);
        var iterations = 4;
        var memorySizeKB = 65536;
        var parallelism = 2;

        // Act
        var key1 = _service.DeriveKey(password1, salt, iterations, memorySizeKB, parallelism);
        var key2 = _service.DeriveKey(password2, salt, iterations, memorySizeKB, parallelism);

        // Assert
        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void DeriveKey_WithDifferentSalts_ShouldReturnDifferentKeys()
    {
        // Arrange
        var password = "TestPassword123!";
        var salt1 = new byte[16];
        var salt2 = new byte[16];
        new Random().NextBytes(salt1);
        new Random().NextBytes(salt2);
        var iterations = 4;
        var memorySizeKB = 65536;
        var parallelism = 2;

        // Act
        var key1 = _service.DeriveKey(password, salt1, iterations, memorySizeKB, parallelism);
        var key2 = _service.DeriveKey(password, salt2, iterations, memorySizeKB, parallelism);

        // Assert
        key1.Should().NotEqual(key2);
    }

    [Fact]
    public void DeriveKey_WithOutParameter_ShouldGenerateRandomSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var key1 = _service.DeriveKey(password, out var salt1);
        var key2 = _service.DeriveKey(password, out var salt2);

        // Assert
        salt1.Length.Should().Be(16);
        salt2.Length.Should().Be(16);
        salt1.Should().NotEqual(salt2); // 随机盐应该不同
        key1.Should().NotEqual(key2); // 不同的盐应该产生不同的密钥
    }

    [Fact]
    public void DeriveKey_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var password = "";
        var salt = new byte[16];

        // Act
        var act = () => _service.DeriveKey(password, salt, 4, 65536, 2);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeriveKey_WithNullPassword_ShouldThrowArgumentException()
    {
        // Arrange
        string password = null!;
        var salt = new byte[16];

        // Act
        var act = () => _service.DeriveKey(password, salt, 4, 65536, 2);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeriveKey_WithDefaultParameters_ShouldUseSecureDefaults()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var key = _service.DeriveKey(password, out var salt);

        // Assert
        key.Should().NotBeNull();
        key.Length.Should().Be(32);
        salt.Length.Should().Be(16);
    }
}
