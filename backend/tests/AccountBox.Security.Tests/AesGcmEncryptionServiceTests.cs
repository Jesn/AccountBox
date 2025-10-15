using AccountBox.Security.Encryption;
using FluentAssertions;
using Xunit;

namespace AccountBox.Security.Tests;

/// <summary>
/// AesGcmEncryptionService 单元测试
/// </summary>
public class AesGcmEncryptionServiceTests
{
    private readonly AesGcmEncryptionService _service;

    public AesGcmEncryptionServiceTests()
    {
        _service = new AesGcmEncryptionService();
    }

    [Fact]
    public void Encrypt_WithValidInput_ShouldReturnCiphertextIVAndTag()
    {
        // Arrange
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        var key = new byte[32]; // 256-bit key
        new Random().NextBytes(key);

        // Act
        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, key);

        // Assert
        ciphertext.Should().NotBeNull();
        ciphertext.Length.Should().Be(plaintext.Length);
        iv.Should().NotBeNull();
        iv.Length.Should().Be(12); // 96 bits
        tag.Should().NotBeNull();
        tag.Length.Should().Be(16); // 128 bits
    }

    [Fact]
    public void Encrypt_MultipleTimes_ShouldGenerateDifferentIVs()
    {
        // Arrange
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        var key = new byte[32];
        new Random().NextBytes(key);

        // Act
        var (_, iv1, _) = _service.Encrypt(plaintext, key);
        var (_, iv2, _) = _service.Encrypt(plaintext, key);

        // Assert
        iv1.Should().NotEqual(iv2); // IVs should be unique
    }

    [Fact]
    public void Decrypt_WithValidCiphertext_ShouldReturnOriginalPlaintext()
    {
        // Arrange
        var originalPlaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World! This is a test message.");
        var key = new byte[32];
        new Random().NextBytes(key);

        var (ciphertext, iv, tag) = _service.Encrypt(originalPlaintext, key);

        // Act
        var decryptedPlaintext = _service.Decrypt(ciphertext, key, iv, tag);

        // Assert
        decryptedPlaintext.Should().Equal(originalPlaintext);
    }

    [Fact]
    public void Decrypt_WithIncorrectKey_ShouldThrowCryptographicException()
    {
        // Arrange
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        var correctKey = new byte[32];
        var incorrectKey = new byte[32];
        new Random().NextBytes(correctKey);
        new Random().NextBytes(incorrectKey);

        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, correctKey);

        // Act
        var act = () => _service.Decrypt(ciphertext, incorrectKey, iv, tag);

        // Assert
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithModifiedCiphertext_ShouldThrowCryptographicException()
    {
        // Arrange
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        var key = new byte[32];
        new Random().NextBytes(key);

        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, key);

        // Modify ciphertext
        ciphertext[0] ^= 0xFF;

        // Act
        var act = () => _service.Decrypt(ciphertext, key, iv, tag);

        // Assert
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithModifiedTag_ShouldThrowCryptographicException()
    {
        // Arrange
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        var key = new byte[32];
        new Random().NextBytes(key);

        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, key);

        // Modify tag
        tag[0] ^= 0xFF;

        // Act
        var act = () => _service.Decrypt(ciphertext, key, iv, tag);

        // Assert
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public void Encrypt_WithEmptyPlaintext_ShouldSucceed()
    {
        // Arrange
        var plaintext = Array.Empty<byte>();
        var key = new byte[32];
        new Random().NextBytes(key);

        // Act
        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, key);

        // Assert
        ciphertext.Should().NotBeNull();
        ciphertext.Length.Should().Be(0);
        iv.Length.Should().Be(12);
        tag.Length.Should().Be(16);
    }

    [Fact]
    public void Decrypt_WithEmptyCiphertext_ShouldReturnEmptyPlaintext()
    {
        // Arrange
        var plaintext = Array.Empty<byte>();
        var key = new byte[32];
        new Random().NextBytes(key);

        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, key);

        // Act
        var decrypted = _service.Decrypt(ciphertext, key, iv, tag);

        // Assert
        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void Encrypt_WithLargePlaintext_ShouldSucceed()
    {
        // Arrange
        var plaintext = new byte[1024 * 1024]; // 1MB
        new Random().NextBytes(plaintext);
        var key = new byte[32];
        new Random().NextBytes(key);

        // Act
        var (ciphertext, iv, tag) = _service.Encrypt(plaintext, key);
        var decrypted = _service.Decrypt(ciphertext, key, iv, tag);

        // Assert
        decrypted.Should().Equal(plaintext);
    }

    [Fact]
    public void Encrypt_WithNullPlaintext_ShouldThrowArgumentNullException()
    {
        // Arrange
        byte[] plaintext = null!;
        var key = new byte[32];

        // Act
        var act = () => _service.Encrypt(plaintext, key);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Encrypt_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
        byte[] key = null!;

        // Act
        var act = () => _service.Encrypt(plaintext, key);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
