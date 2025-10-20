namespace AccountBox.Core.Interfaces;

/// <summary>
/// Vault 管理器接口，负责管理信封加密的 VaultKey 和主密码验证
/// </summary>
public interface IVaultManager
{
    /// <summary>
    /// 初始化 Vault，使用主密码创建新的 VaultKey
    /// </summary>
    /// <param name="masterPassword">用户设置的主密码</param>
    /// <returns>加密后的 VaultKey、盐、IV、Tag 和 Argon2 参数</returns>
    (byte[] EncryptedVaultKey, byte[] VaultKeyIV, byte[] VaultKeyTag, byte[] Argon2Salt,
     int Argon2Iterations, int Argon2MemorySize, int Argon2Parallelism) Initialize(string masterPassword);

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
    /// <exception cref="System.Security.Cryptography.CryptographicException">密码错误或解密失败时抛出</exception>
    byte[] Unlock(
        string masterPassword,
        byte[] encryptedVaultKey,
        byte[] vaultKeyIV,
        byte[] vaultKeyTag,
        byte[] argon2Salt,
        int argon2Iterations,
        int argon2MemorySize,
        int argon2Parallelism);

    /// <summary>
    /// 锁定 Vault，清除内存中的 VaultKey
    /// </summary>
    void Lock();

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
    /// <exception cref="System.Security.Cryptography.CryptographicException">旧密码错误时抛出</exception>
    (byte[] EncryptedVaultKey, byte[] VaultKeyIV, byte[] VaultKeyTag, byte[] Argon2Salt,
     int Argon2Iterations, int Argon2MemorySize, int Argon2Parallelism) ChangeMasterPassword(
        string oldMasterPassword,
        string newMasterPassword,
        byte[] encryptedVaultKey,
        byte[] vaultKeyIV,
        byte[] vaultKeyTag,
        byte[] argon2Salt,
        int argon2Iterations,
        int argon2MemorySize,
        int argon2Parallelism);
}
