using System.Security.Cryptography;
using System.Text;
using AccountBox.Core.Services;

namespace AccountBox.Api.Services;

/// <summary>
/// API密钥服务实现
/// </summary>
public class ApiKeyService : IApiKeyService
{
    /// <summary>
    /// 生成新的API密钥
    /// </summary>
    public string GenerateApiKey()
    {
        // 生成32个随机字符
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var stringBuilder = new StringBuilder(32);
        foreach (var b in randomBytes)
        {
            stringBuilder.Append(chars[b % chars.Length]);
        }

        return $"sk_{stringBuilder}";
    }

    /// <summary>
    /// 验证API密钥
    /// </summary>
    public bool VerifyApiKey(string providedKey, string storedHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(providedKey, storedHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 哈希API密钥
    /// </summary>
    public string HashApiKey(string apiKey)
    {
        return BCrypt.Net.BCrypt.HashPassword(apiKey, workFactor: 12);
    }
}
