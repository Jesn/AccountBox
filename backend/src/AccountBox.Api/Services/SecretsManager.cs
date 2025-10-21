using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

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
    /// 获取或生成主密码哈希
    /// </summary>
    /// <returns>主密码的 BCrypt 哈希值</returns>
    public string GetOrGenerateMasterPasswordHash()
    {
        // 优先级1: 环境变量（支持明文密码，自动转换为哈希）
        var envPassword = Environment.GetEnvironmentVariable("MASTER_PASSWORD");
        if (!string.IsNullOrWhiteSpace(envPassword))
        {
            _logger.LogInformation("使用环境变量中的主密码");
            // 如果环境变量是明文密码，返回其哈希值（不存储）
            return BCrypt.Net.BCrypt.HashPassword(envPassword, workFactor: 12);
        }

        // 优先级2: 持久化文件
        if (File.Exists(_masterPasswordPath))
        {
            try
            {
                var storedHash = File.ReadAllText(_masterPasswordPath).Trim();
                if (!string.IsNullOrWhiteSpace(storedHash))
                {
                    // 检查是否是 BCrypt 哈希（以 $2a$, $2b$, $2y$ 开头）
                    if (storedHash.StartsWith("$2"))
                    {
                        _logger.LogInformation("从持久化文件加载主密码哈希");
                        return storedHash;
                    }
                    else
                    {
                        // 迁移：将明文密码转换为哈希
                        _logger.LogWarning("检测到明文主密码，正在迁移到哈希存储...");
                        var hash = BCrypt.Net.BCrypt.HashPassword(storedHash, workFactor: 12);
                        File.WriteAllText(_masterPasswordPath, hash);
                        _logger.LogInformation("主密码已成功迁移到哈希存储");
                        return hash;
                    }
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
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

        try
        {
            File.WriteAllText(_masterPasswordPath, newHash);
            _logger.LogWarning("=".PadRight(80, '='));
            _logger.LogWarning("首次启动 - 已生成随机主密码");
            _logger.LogWarning("主密码: {Password}", newPassword);
            _logger.LogWarning("请妥善保存此密码！密码哈希已保存到: {Path}", _masterPasswordPath);
            _logger.LogWarning("注意：密码哈希使用 BCrypt 算法存储，无法反向解密");
            _logger.LogWarning("=".PadRight(80, '='));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存主密码哈希失败");
            _logger.LogWarning("临时主密码（仅本次有效）: {Password}", newPassword);
        }

        return newHash;
    }

    /// <summary>
    /// 验证主密码
    /// </summary>
    /// <param name="inputPassword">用户输入的密码</param>
    /// <param name="storedHash">存储的密码哈希</param>
    /// <returns>密码是否正确</returns>
    public bool VerifyMasterPassword(string inputPassword, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(inputPassword) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        try
        {
            // 使用 BCrypt 验证密码（内置恒定时间比对）
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证主密码时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 生成安全的随机密钥（Base64编码）
    /// </summary>
    /// <param name="length">原始字节长度（生成前的字节数）</param>
    /// <returns>Base64 编码的密钥字符串</returns>
    private string GenerateSecureKey(int length)
    {
        // 确保至少生成 32 字节（256 位）以满足 HS256 要求
        if (length < 32)
        {
            _logger.LogWarning("密钥长度 {Length} 小于 32 字节，已自动调整为 32 字节", length);
            length = 32;
        }

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
