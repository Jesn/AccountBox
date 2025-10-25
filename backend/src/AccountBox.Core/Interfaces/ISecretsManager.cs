namespace AccountBox.Core.Interfaces;

/// <summary>
/// 密钥管理服务接口
/// </summary>
public interface ISecretsManager
{
    /// <summary>
    /// 获取或生成 JWT 密钥
    /// </summary>
    /// <returns>JWT 密钥字符串</returns>
    string GetOrGenerateJwtSecretKey();

    /// <summary>
    /// 获取或生成主密码哈希
    /// </summary>
    /// <returns>主密码哈希值</returns>
    string GetOrGenerateMasterPasswordHash();

    /// <summary>
    /// 验证主密码
    /// </summary>
    /// <param name="inputPassword">用户输入的密码</param>
    /// <param name="storedHash">存储的密码哈希</param>
    /// <returns>密码是否正确</returns>
    bool VerifyMasterPassword(string inputPassword, string storedHash);

    /// <summary>
    /// 轮换 JWT 密钥（可选功能）
    /// </summary>
    string RotateJwtSecretKey();

    /// <summary>
    /// 获取密钥信息（用于诊断）
    /// </summary>
    Dictionary<string, object> GetSecretsInfo();
}