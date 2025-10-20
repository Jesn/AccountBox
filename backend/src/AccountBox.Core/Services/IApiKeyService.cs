namespace AccountBox.Core.Services;

/// <summary>
/// API密钥服务接口
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// 生成新的API密钥
    /// </summary>
    /// <returns>生成的密钥明文（"sk_" + 32字符）</returns>
    string GenerateApiKey();

    /// <summary>
    /// 验证API密钥
    /// </summary>
    /// <param name="providedKey">提供的密钥明文</param>
    /// <param name="storedHash">存储的BCrypt哈希值</param>
    /// <returns>验证是否通过</returns>
    bool VerifyApiKey(string providedKey, string storedHash);

    /// <summary>
    /// 哈希API密钥
    /// </summary>
    /// <param name="apiKey">密钥明文</param>
    /// <returns>BCrypt哈希值</returns>
    string HashApiKey(string apiKey);
}
