namespace AccountBox.Core.Interfaces;

/// <summary>
/// 加密服务接口，提供 AES-256-GCM 加密和解密功能
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// 使用 AES-256-GCM 加密数据
    /// </summary>
    /// <param name="plaintext">明文数据</param>
    /// <param name="key">256位（32字节）加密密钥</param>
    /// <returns>包含密文、IV和认证标签的元组</returns>
    (byte[] Ciphertext, byte[] IV, byte[] Tag) Encrypt(byte[] plaintext, byte[] key);

    /// <summary>
    /// 使用 AES-256-GCM 解密数据
    /// </summary>
    /// <param name="ciphertext">密文数据</param>
    /// <param name="key">256位（32字节）加密密钥</param>
    /// <param name="iv">初始化向量</param>
    /// <param name="tag">认证标签</param>
    /// <returns>解密后的明文</returns>
    /// <exception cref="System.Security.Cryptography.CryptographicException">认证失败或解密错误时抛出</exception>
    byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv, byte[] tag);
}
