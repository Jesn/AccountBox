namespace AccountBox.Core.Interfaces;

/// <summary>
/// 主密码密钥管理服务接口。
/// </summary>
public interface ISecretsManager
{
    /// <summary>
    /// 获取或生成主密码哈希。
    /// </summary>
    string GetOrGenerateMasterPasswordHash();

    /// <summary>
    /// 验证主密码。
    /// </summary>
    bool VerifyMasterPassword(string inputPassword, string storedHash);

    /// <summary>
    /// 获取密钥存储信息。
    /// </summary>
    Dictionary<string, object> GetSecretsInfo();
}