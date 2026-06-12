using System.Security.Cryptography;
using System.Text;
using AccountBox.Core.Configuration;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace AccountBox.Security;

/// <summary>
/// 主密码密钥管理服务。
/// </summary>
public class SecretsManager : ISecretsManager
{
    private readonly ILogger<SecretsManager> _logger;
    private readonly SecurityStorageOptions _storageOptions;

    public SecretsManager(ILogger<SecretsManager> logger, SecurityStorageOptions storageOptions)
    {
        _logger = logger;
        _storageOptions = storageOptions;
        EnsureSecretsDirectoryExists();
    }

    /// <summary>
    /// 获取或生成主密码哈希。
    /// </summary>
    public string GetOrGenerateMasterPasswordHash()
    {
        var envPassword = Environment.GetEnvironmentVariable(AccountBoxEnvironment.MasterPassword);
        if (!string.IsNullOrWhiteSpace(envPassword))
        {
            _logger.LogInformation("使用环境变量中的主密码");
            return BCrypt.Net.BCrypt.HashPassword(envPassword, workFactor: 12);
        }

        if (File.Exists(_storageOptions.MasterPasswordPath))
        {
            try
            {
                var storedHash = File.ReadAllText(_storageOptions.MasterPasswordPath).Trim();
                if (!string.IsNullOrWhiteSpace(storedHash))
                {
                    if (storedHash.StartsWith("$2"))
                    {
                        _logger.LogInformation("从持久化文件加载主密码哈希");
                        return storedHash;
                    }

                    _logger.LogWarning("检测到明文主密码，正在迁移到哈希存储...");
                    var hash = BCrypt.Net.BCrypt.HashPassword(storedHash, workFactor: 12);
                    File.WriteAllText(_storageOptions.MasterPasswordPath, hash);
                    SetOwnerOnlyFileMode(_storageOptions.MasterPasswordPath);
                    _logger.LogInformation("主密码已成功迁移到哈希存储");
                    return hash;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取主密码文件失败，将生成新密码");
            }
        }

        _logger.LogWarning("未找到主密码，正在生成随机密码...");
        var newPassword = GenerateReadablePassword(16);
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

        try
        {
            File.WriteAllText(_storageOptions.MasterPasswordPath, newHash);
            SetOwnerOnlyFileMode(_storageOptions.MasterPasswordPath);
            const string separator = "================================================================================";
            _logger.LogWarning(separator);
            _logger.LogWarning("首次启动 - 已生成随机主密码");
            _logger.LogWarning("主密码: {Password}", newPassword);
            _logger.LogWarning("请妥善保存此密码！密码哈希已保存到: {Path}", _storageOptions.MasterPasswordPath);
            _logger.LogWarning("注意：密码哈希使用 BCrypt 算法存储，无法反向解密");
            _logger.LogWarning(separator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存主密码哈希失败");
            _logger.LogWarning("临时主密码（仅本次有效）: {Password}", newPassword);
        }

        return newHash;
    }

    /// <summary>
    /// 验证主密码。
    /// </summary>
    public bool VerifyMasterPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证主密码时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 获取密钥存储信息。
    /// </summary>
    public Dictionary<string, object> GetSecretsInfo()
    {
        return new Dictionary<string, object>
        {
            { "SecretsDirectory", _storageOptions.SecretsDirectory },
            { "MasterPasswordExists", File.Exists(_storageOptions.MasterPasswordPath) },
            { "MasterPasswordFromEnv", !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(AccountBoxEnvironment.MasterPassword)) }
        };
    }

    private string GenerateReadablePassword(int length)
    {
        const string upperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*-_=+";
        const string allChars = upperCase + lowerCase + digits + special;

        var password = new StringBuilder();
        using (var rng = RandomNumberGenerator.Create())
        {
            password.Append(GetRandomChar(rng, upperCase));
            password.Append(GetRandomChar(rng, lowerCase));
            password.Append(GetRandomChar(rng, digits));
            password.Append(GetRandomChar(rng, special));

            for (var i = 4; i < length; i++)
            {
                password.Append(GetRandomChar(rng, allChars));
            }
        }

        return new string(password.ToString().OrderBy(_ => Guid.NewGuid()).ToArray());
    }

    private static char GetRandomChar(RandomNumberGenerator rng, string chars)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomIndex = BitConverter.ToUInt32(randomBytes, 0) % chars.Length;
        return chars[(int)randomIndex];
    }

    private void EnsureSecretsDirectoryExists()
    {
        try
        {
            if (Directory.Exists(_storageOptions.SecretsDirectory))
            {
                return;
            }

            Directory.CreateDirectory(_storageOptions.SecretsDirectory);
            _logger.LogInformation("创建密钥存储目录: {Path}", _storageOptions.SecretsDirectory);
            SetOwnerOnlyDirectoryMode(_storageOptions.SecretsDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建密钥存储目录失败: {Path}", _storageOptions.SecretsDirectory);
        }
    }

    private void SetOwnerOnlyDirectoryMode(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            File.SetUnixFileMode(path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "设置密钥目录权限失败");
        }
    }

    private void SetOwnerOnlyFileMode(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "设置密钥文件权限失败");
        }
    }
}