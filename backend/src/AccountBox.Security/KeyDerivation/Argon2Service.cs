using System.Security.Cryptography;
using Konscious.Security.Cryptography;

namespace AccountBox.Security.KeyDerivation;

/// <summary>
/// Argon2id 密钥派生服务，用于从主密码派生加密密钥
/// Argon2id 是 2015 年密码哈希竞赛的获胜者，提供了针对侧信道攻击和 GPU 破解的最佳保护
/// </summary>
public class Argon2Service
{
    // 推荐参数（基于 OWASP 2023 指南）
    private const int DefaultIterations = 4;           // 迭代次数
    private const int DefaultMemorySize = 65536;       // 内存大小：64 MB (in KB)
    private const int DefaultParallelism = 2;          // 并行度（线程数）
    private const int DefaultSaltSize = 16;            // 盐长度：128 bits
    private const int DefaultKeySize = 32;             // 输出密钥长度：256 bits (AES-256)

    /// <summary>
    /// 从密码派生密钥，使用随机生成的盐
    /// </summary>
    /// <param name="password">主密码</param>
    /// <param name="salt">输出：生成的随机盐</param>
    /// <param name="iterations">迭代次数（可选，默认 4）</param>
    /// <param name="memorySizeKB">内存大小 KB（可选，默认 64 MB）</param>
    /// <param name="parallelism">并行度（可选，默认 2）</param>
    /// <returns>派生的 256 位密钥</returns>
    public byte[] DeriveKey(
        string password,
        out byte[] salt,
        int? iterations = null,
        int? memorySizeKB = null,
        int? parallelism = null)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        // 生成随机盐
        salt = RandomNumberGenerator.GetBytes(DefaultSaltSize);

        return DeriveKey(
            password,
            salt,
            iterations ?? DefaultIterations,
            memorySizeKB ?? DefaultMemorySize,
            parallelism ?? DefaultParallelism);
    }

    /// <summary>
    /// 从密码派生密钥，使用提供的盐（用于验证）
    /// </summary>
    /// <param name="password">主密码</param>
    /// <param name="salt">已存在的盐</param>
    /// <param name="iterations">迭代次数</param>
    /// <param name="memorySizeKB">内存大小 KB</param>
    /// <param name="parallelism">并行度</param>
    /// <returns>派生的 256 位密钥</returns>
    public byte[] DeriveKey(
        string password,
        byte[] salt,
        int iterations,
        int memorySizeKB,
        int parallelism)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password cannot be null or empty", nameof(password));
        }

        if (salt == null || salt.Length < 8)
        {
            throw new ArgumentException("Salt must be at least 8 bytes", nameof(salt));
        }

        if (iterations < 1)
        {
            throw new ArgumentException("Iterations must be at least 1", nameof(iterations));
        }

        if (memorySizeKB < 8)
        {
            throw new ArgumentException("Memory size must be at least 8 KB", nameof(memorySizeKB));
        }

        if (parallelism < 1)
        {
            throw new ArgumentException("Parallelism must be at least 1", nameof(parallelism));
        }

        using var argon2 = new Argon2id(System.Text.Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            MemorySize = memorySizeKB,
            Iterations = iterations
        };

        return argon2.GetBytes(DefaultKeySize);
    }

    /// <summary>
    /// 验证密码是否匹配
    /// </summary>
    /// <param name="password">要验证的密码</param>
    /// <param name="expectedKey">预期的派生密钥</param>
    /// <param name="salt">盐</param>
    /// <param name="iterations">迭代次数</param>
    /// <param name="memorySizeKB">内存大小 KB</param>
    /// <param name="parallelism">并行度</param>
    /// <returns>密码是否匹配</returns>
    public bool VerifyPassword(
        string password,
        byte[] expectedKey,
        byte[] salt,
        int iterations,
        int memorySizeKB,
        int parallelism)
    {
        if (expectedKey == null || expectedKey.Length != DefaultKeySize)
        {
            throw new ArgumentException($"Expected key must be {DefaultKeySize} bytes", nameof(expectedKey));
        }

        var derivedKey = DeriveKey(password, salt, iterations, memorySizeKB, parallelism);

        // 使用常量时间比较防止时序攻击
        return CryptographicOperations.FixedTimeEquals(derivedKey, expectedKey);
    }
}
