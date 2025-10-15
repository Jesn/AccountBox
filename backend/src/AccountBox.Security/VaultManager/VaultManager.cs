using System.Security.Cryptography;
using AccountBox.Core.Interfaces;
using AccountBox.Security.KeyDerivation;

namespace AccountBox.Security.VaultManager;

/// <summary>
/// Vault 管理器实现，负责信封加密方案
/// 架构：主密码 → Argon2id → KEK (Key Encryption Key) → 加密 VaultKey → VaultKey 用于加密业务数据
/// </summary>
public class VaultManager : IVaultManager
{
    private readonly Argon2Service _argon2Service;
    private readonly IEncryptionService _encryptionService;

    // VaultKey 是随机生成的 256 位密钥，用于加密所有业务数据
    private const int VaultKeySize = 32; // 256 bits

    public VaultManager(Argon2Service argon2Service, IEncryptionService encryptionService)
    {
        _argon2Service = argon2Service ?? throw new ArgumentNullException(nameof(argon2Service));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
    }

    /// <summary>
    /// 初始化 Vault，使用主密码创建新的 VaultKey
    /// </summary>
    /// <param name="masterPassword">用户设置的主密码</param>
    /// <returns>加密后的 VaultKey、盐、IV、Tag 和 Argon2 参数</returns>
    /// <exception cref="ArgumentException">masterPassword 为空</exception>
    public (byte[] EncryptedVaultKey, byte[] VaultKeyIV, byte[] VaultKeyTag, byte[] Argon2Salt,
            int Argon2Iterations, int Argon2MemorySize, int Argon2Parallelism) Initialize(string masterPassword)
    {
        if (string.IsNullOrWhiteSpace(masterPassword))
        {
            throw new ArgumentException("Master password cannot be null or empty", nameof(masterPassword));
        }

        // 1. 生成随机的 VaultKey（数据加密密钥 DEK）
        var vaultKey = RandomNumberGenerator.GetBytes(VaultKeySize);

        // 2. 从主密码派生 KEK (Key Encryption Key)
        var kek = _argon2Service.DeriveKey(
            masterPassword,
            out var argon2Salt,
            iterations: 4,        // 保存这些参数以便后续解锁
            memorySizeKB: 65536,  // 64 MB
            parallelism: 2);

        // 3. 使用 KEK 加密 VaultKey（信封加密）
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag) = _encryptionService.Encrypt(vaultKey, kek);

        // 清除敏感数据
        CryptographicOperations.ZeroMemory(vaultKey);
        CryptographicOperations.ZeroMemory(kek);

        return (
            encryptedVaultKey,
            vaultKeyIV,
            vaultKeyTag,
            argon2Salt,
            Argon2Iterations: 4,
            Argon2MemorySize: 65536,
            Argon2Parallelism: 2
        );
    }

    /// <summary>
    /// 使用主密码解锁 Vault
    /// </summary>
    /// <param name="masterPassword">用户输入的主密码</param>
    /// <param name="encryptedVaultKey">加密的 VaultKey</param>
    /// <param name="vaultKeyIV">VaultKey 加密的 IV</param>
    /// <param name="vaultKeyTag">VaultKey 加密的 Tag</param>
    /// <param name="argon2Salt">Argon2 盐</param>
    /// <param name="argon2Iterations">Argon2 迭代次数</param>
    /// <param name="argon2MemorySize">Argon2 内存大小（KB）</param>
    /// <param name="argon2Parallelism">Argon2 并行度</param>
    /// <returns>解密后的 VaultKey（会话密钥）</returns>
    /// <exception cref="ArgumentException">参数无效</exception>
    /// <exception cref="CryptographicException">密码错误或解密失败</exception>
    public byte[] Unlock(
        string masterPassword,
        byte[] encryptedVaultKey,
        byte[] vaultKeyIV,
        byte[] vaultKeyTag,
        byte[] argon2Salt,
        int argon2Iterations,
        int argon2MemorySize,
        int argon2Parallelism)
    {
        if (string.IsNullOrWhiteSpace(masterPassword))
        {
            throw new ArgumentException("Master password cannot be null or empty", nameof(masterPassword));
        }

        if (encryptedVaultKey == null)
        {
            throw new ArgumentNullException(nameof(encryptedVaultKey));
        }

        if (vaultKeyIV == null)
        {
            throw new ArgumentNullException(nameof(vaultKeyIV));
        }

        if (vaultKeyTag == null)
        {
            throw new ArgumentNullException(nameof(vaultKeyTag));
        }

        if (argon2Salt == null)
        {
            throw new ArgumentNullException(nameof(argon2Salt));
        }

        byte[]? kek = null;

        try
        {
            // 1. 从主密码派生 KEK
            kek = _argon2Service.DeriveKey(
                masterPassword,
                argon2Salt,
                argon2Iterations,
                argon2MemorySize,
                argon2Parallelism);

            // 2. 使用 KEK 解密 VaultKey
            // 如果密码错误，这里会抛出 CryptographicException
            var vaultKey = _encryptionService.Decrypt(
                encryptedVaultKey,
                kek,
                vaultKeyIV,
                vaultKeyTag);

            return vaultKey;
        }
        catch (CryptographicException)
        {
            // 密码错误或数据损坏
            throw new CryptographicException("Invalid master password or corrupted vault data");
        }
        finally
        {
            // 清除 KEK
            if (kek != null)
            {
                CryptographicOperations.ZeroMemory(kek);
            }
        }
    }

    /// <summary>
    /// 锁定 Vault，清除内存中的 VaultKey
    /// 注意：此方法在当前实现中主要用于文档完整性
    /// 实际的 VaultKey 会话管理应该在更高层（如 VaultService）中处理
    /// </summary>
    public void Lock()
    {
        // VaultKey 的生命周期管理在调用层（VaultService）
        // 这里保留接口以符合契约
    }

    /// <summary>
    /// 修改主密码
    /// </summary>
    /// <param name="oldMasterPassword">旧主密码</param>
    /// <param name="newMasterPassword">新主密码</param>
    /// <param name="encryptedVaultKey">当前加密的 VaultKey</param>
    /// <param name="vaultKeyIV">VaultKey 加密的 IV</param>
    /// <param name="vaultKeyTag">VaultKey 加密的 Tag</param>
    /// <param name="argon2Salt">当前 Argon2 盐</param>
    /// <param name="argon2Iterations">当前 Argon2 迭代次数</param>
    /// <param name="argon2MemorySize">当前 Argon2 内存大小（KB）</param>
    /// <param name="argon2Parallelism">当前 Argon2 并行度</param>
    /// <returns>新的加密 VaultKey、盐、IV、Tag 和 Argon2 参数</returns>
    /// <exception cref="ArgumentException">参数无效</exception>
    /// <exception cref="CryptographicException">旧密码错误</exception>
    public (byte[] EncryptedVaultKey, byte[] VaultKeyIV, byte[] VaultKeyTag, byte[] Argon2Salt,
            int Argon2Iterations, int Argon2MemorySize, int Argon2Parallelism) ChangeMasterPassword(
        string oldMasterPassword,
        string newMasterPassword,
        byte[] encryptedVaultKey,
        byte[] vaultKeyIV,
        byte[] vaultKeyTag,
        byte[] argon2Salt,
        int argon2Iterations,
        int argon2MemorySize,
        int argon2Parallelism)
    {
        if (string.IsNullOrWhiteSpace(oldMasterPassword))
        {
            throw new ArgumentException("Old master password cannot be null or empty", nameof(oldMasterPassword));
        }

        if (string.IsNullOrWhiteSpace(newMasterPassword))
        {
            throw new ArgumentException("New master password cannot be null or empty", nameof(newMasterPassword));
        }

        byte[]? vaultKey = null;
        byte[]? newKek = null;

        try
        {
            // 1. 使用旧密码解锁，获取 VaultKey
            vaultKey = Unlock(
                oldMasterPassword,
                encryptedVaultKey,
                vaultKeyIV,
                vaultKeyTag,
                argon2Salt,
                argon2Iterations,
                argon2MemorySize,
                argon2Parallelism);

            // 2. 从新密码派生新的 KEK
            newKek = _argon2Service.DeriveKey(
                newMasterPassword,
                out var newArgon2Salt,
                iterations: 4,
                memorySizeKB: 65536,
                parallelism: 2);

            // 3. 使用新 KEK 重新加密 VaultKey
            var (newEncryptedVaultKey, newVaultKeyIV, newVaultKeyTag) = _encryptionService.Encrypt(vaultKey, newKek);

            return (
                newEncryptedVaultKey,
                newVaultKeyIV,
                newVaultKeyTag,
                newArgon2Salt,
                Argon2Iterations: 4,
                Argon2MemorySize: 65536,
                Argon2Parallelism: 2
            );
        }
        finally
        {
            // 清除敏感数据
            if (vaultKey != null)
            {
                CryptographicOperations.ZeroMemory(vaultKey);
            }

            if (newKek != null)
            {
                CryptographicOperations.ZeroMemory(newKek);
            }
        }
    }
}
