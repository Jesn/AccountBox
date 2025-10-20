using System.Security.Cryptography;
using AccountBox.Core.Interfaces;

namespace AccountBox.Security.Encryption;

/// <summary>
/// AES-256-GCM 加密服务实现
/// GCM (Galois/Counter Mode) 提供认证加密，确保数据的机密性和完整性
/// </summary>
public class AesGcmEncryptionService : IEncryptionService
{
    private const int KeySize = 32;      // 256 bits = 32 bytes
    private const int NonceSize = 12;    // 96 bits = 12 bytes (推荐的 GCM nonce 大小)
    private const int TagSize = 16;      // 128 bits = 16 bytes (认证标签)

    /// <summary>
    /// 使用 AES-256-GCM 加密数据
    /// </summary>
    /// <param name="plaintext">明文数据</param>
    /// <param name="key">256位（32字节）加密密钥</param>
    /// <returns>包含密文、IV（Nonce）和认证标签的元组</returns>
    /// <exception cref="ArgumentNullException">plaintext 或 key 为 null</exception>
    /// <exception cref="ArgumentException">key 长度不是 32 字节</exception>
    public (byte[] Ciphertext, byte[] IV, byte[] Tag) Encrypt(byte[] plaintext, byte[] key)
    {
        if (plaintext == null)
        {
            throw new ArgumentNullException(nameof(plaintext));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (key.Length != KeySize)
        {
            throw new ArgumentException($"Key must be {KeySize} bytes (256 bits)", nameof(key));
        }

        // 生成随机 nonce (IV)
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);

        // 分配缓冲区
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        // 执行加密
        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return (ciphertext, nonce, tag);
    }

    /// <summary>
    /// 使用 AES-256-GCM 解密数据
    /// </summary>
    /// <param name="ciphertext">密文数据</param>
    /// <param name="key">256位（32字节）加密密钥</param>
    /// <param name="iv">初始化向量（Nonce）</param>
    /// <param name="tag">认证标签</param>
    /// <returns>解密后的明文</returns>
    /// <exception cref="ArgumentNullException">任何参数为 null</exception>
    /// <exception cref="ArgumentException">参数长度不正确</exception>
    /// <exception cref="CryptographicException">认证失败或解密错误</exception>
    public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag)
    {
        if (ciphertext == null)
        {
            throw new ArgumentNullException(nameof(ciphertext));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (iv == null)
        {
            throw new ArgumentNullException(nameof(iv));
        }

        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        if (key.Length != KeySize)
        {
            throw new ArgumentException($"Key must be {KeySize} bytes (256 bits)", nameof(key));
        }

        if (iv.Length != NonceSize)
        {
            throw new ArgumentException($"IV (nonce) must be {NonceSize} bytes", nameof(iv));
        }

        if (tag.Length != TagSize)
        {
            throw new ArgumentException($"Tag must be {TagSize} bytes", nameof(tag));
        }

        // 分配明文缓冲区
        var plaintext = new byte[ciphertext.Length];

        // 执行解密和认证
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(iv, ciphertext, tag, plaintext);

        return plaintext;
    }
}
