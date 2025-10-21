using System.Security.Cryptography;
using System.Text;

namespace AccountBox.Api.Services;

/// <summary>
/// 密钥管理服务 - 负责生成、存储和加载应用密钥
/// </summary>
public class SecretsManager
{
    private readonly ILogger<SecretsManager> _logger;
    private readonly string _secretsDirectory;
    private readonly string _jwtKeyPath;
    private readonly string _masterPasswordPath;

    public SecretsManager(ILogger<SecretsManager> logger, IConfiguration configuration)
    {
        _logger = logger;

        // 从环境变量或配置获取密钥存储目录
        var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
                       ?? Path.Combine(Directory.GetCurrentDirectory(), "data");

        _secretsDirectory = Path.Combine(dataPath, ".secrets");
        _jwtKeyPath = Path.Combine(_secretsDirectory, "jwt.key");
        _masterPasswordPath = Path.Combine(_secretsDirectory, "master.key");

        // 确保密钥目录存在
        EnsureSecretsDirectoryExists();
    }

    /// <summary>
    /// 获取或生成 JWT 密钥
    /// </summary>
    public string GetOrGenerateJwtSecretKey()
    {
        // 优先级1: 环境变量
        var envKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            _logger.LogInformation("使用环境变量中的 JWT 密钥");
            return envKey;
        }

        // 优先级2: 持久化文件
        if (File.Exists(_jwtKeyPath))
        {
            try
            {
                var key = File.ReadAllText(_jwtKeyPath).Trim();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogInformation("从持久化文件加载 JWT 密钥: {Path}", _jwtKeyPath);
                    return key;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取 JWT 密钥文件失败，将生成新密钥");
            }
        }

        // 优先级3: 自动生成
        _logger.LogWarning("未找到 JWT 密钥，正在生成新的随机密钥...");
        var newKey = GenerateSecureKey(64); // 512位密钥

        try
        {
            File.WriteAllText(_jwtKeyPath, newKey);
            _logger.LogInformation("JWT 密钥已生成并保存到: {Path}", _jwtKeyPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存 JWT 密钥失败，密钥仅在内存中有效");
        }

        return newKey;
    }

    /// <summary>
    /// 获取或生成主密码
    /// </summary>
    public string GetOrGenerateMasterPassword()
    {
        // 优先级1: 环境变量
        var envPassword = Environment.GetEnvironmentVariable("MASTER_PASSWORD");
        if (!string.IsNullOrWhiteSpace(envPassword))
        {
            _logger.LogInformation("使用环境变量中的主密码");
            return envPassword;
        }

        // 优先级2: 持久化文件
        if (File.Exists(_masterPasswordPath))
        {
            try
            {
                var password = File.ReadAllText(_masterPasswordPath).Trim();
                if (!string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogInformation("从持久化文件加载主密码");
                    return password;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取主密码文件失败，将生成新密码");
            }
        }

        // 优先级3: 生成随机密码并显示
        _logger.LogWarning("未找到主密码，正在生成随机密码...");
        var newPassword = GenerateReadablePassword(16);

        try
        {
            File.WriteAllText(_masterPasswordPath, newPassword);
            _logger.LogWarning("=".PadRight(80, '='));
            _logger.LogWarning("首次启动 - 已生成随机主密码");
            _logger.LogWarning("主密码: {Password}", newPassword);
            _logger.LogWarning("请妥善保存此密码！密码已保存到: {Path}", _masterPasswordPath);
            _logger.LogWarning("=".PadRight(80, '='));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存主密码失败");
            _logger.LogWarning("临时主密码（仅本次有效）: {Password}", newPassword);
        }

        return newPassword;
    }

    /// <summary>
    /// 生成安全的随机密钥（Base64编码）
    /// </summary>
    private string GenerateSecureKey(int length)
    {
        var key = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }

    /// <summary>
    /// 生成可读的随机密码（包含大小写字母、数字和特殊字符）
    /// </summary>
    private string GenerateReadablePassword(int length)
    {
        const string upperCase = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // 排除易混淆的 I, O
        const string lowerCase = "abcdefghijkmnopqrstuvwxyz"; // 排除易混淆的 l
        const string digits = "23456789"; // 排除易混淆的 0, 1
        const string special = "!@#$%^&*-_=+";
        const string allChars = upperCase + lowerCase + digits + special;

        var password = new StringBuilder();
        using (var rng = RandomNumberGenerator.Create())
        {
            // 确保至少包含每种类型的字符
            password.Append(GetRandomChar(rng, upperCase));
            password.Append(GetRandomChar(rng, lowerCase));
            password.Append(GetRandomChar(rng, digits));
            password.Append(GetRandomChar(rng, special));

            // 填充剩余长度
            for (int i = 4; i < length; i++)
            {
                password.Append(GetRandomChar(rng, allChars));
            }
        }

        // 打乱顺序
        return new string(password.ToString().OrderBy(_ => Guid.NewGuid()).ToArray());
    }

    /// <summary>
    /// 从字符集中随机选择一个字符
    /// </summary>
    private char GetRandomChar(RandomNumberGenerator rng, string chars)
    {
        var randomBytes = new byte[4];
        rng.GetBytes(randomBytes);
        var randomIndex = BitConverter.ToUInt32(randomBytes, 0) % chars.Length;
        return chars[(int)randomIndex];
    }

    /// <summary>
    /// 确保密钥目录存在
    /// </summary>
    private void EnsureSecretsDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(_secretsDirectory))
            {
                Directory.CreateDirectory(_secretsDirectory);
                _logger.LogInformation("创建密钥存储目录: {Path}", _secretsDirectory);

                // 设置目录权限（仅限 Unix 系统）
                if (!OperatingSystem.IsWindows())
                {
                    try
                    {
                        // 设置为 700 权限（仅所有者可读写执行）
                        File.SetUnixFileMode(_secretsDirectory,
                            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "设置密钥目录权限失败");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建密钥存储目录失败: {Path}", _secretsDirectory);
        }
    }

    /// <summary>
    /// 轮换 JWT 密钥（可选功能）
    /// </summary>
    public string RotateJwtSecretKey()
    {
        _logger.LogWarning("开始轮换 JWT 密钥...");

        // 备份旧密钥
        if (File.Exists(_jwtKeyPath))
        {
            var backupPath = $"{_jwtKeyPath}.{DateTime.UtcNow:yyyyMMddHHmmss}.bak";
            File.Copy(_jwtKeyPath, backupPath);
            _logger.LogInformation("旧密钥已备份到: {Path}", backupPath);
        }

        // 生成新密钥
        var newKey = GenerateSecureKey(64);
        File.WriteAllText(_jwtKeyPath, newKey);
        _logger.LogInformation("新 JWT 密钥已生成并保存");

        return newKey;
    }

    /// <summary>
    /// 获取密钥信息（用于诊断）
    /// </summary>
    public Dictionary<string, object> GetSecretsInfo()
    {
        return new Dictionary<string, object>
        {
            { "SecretsDirectory", _secretsDirectory },
            { "JwtKeyExists", File.Exists(_jwtKeyPath) },
            { "MasterPasswordExists", File.Exists(_masterPasswordPath) },
            { "JwtKeyFromEnv", !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JWT_SECRET_KEY")) },
            { "MasterPasswordFromEnv", !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MASTER_PASSWORD")) }
        };
    }
}
