using System.Security.Cryptography;
using System.Text.Json;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Auth;
using Microsoft.Extensions.Logging;

namespace AccountBox.Security;

/// <summary>
/// JWT密钥轮换服务实现
/// 负责密钥的生成、存储、轮换和版本管理
/// </summary>
public class JwtKeyRotationService : IJwtKeyRotationService
{
    private readonly ILogger<JwtKeyRotationService> _logger;
    private readonly string _keyStorePath;
    private readonly string _secretsDirectory;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private JwtKeyStore? _cachedKeyStore;
    private DateTime _lastCacheTime;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public JwtKeyRotationService(ILogger<JwtKeyRotationService> logger)
    {
        _logger = logger;

        // 密钥存储路径（与SecretsManager一致）
        var dataPath = Environment.GetEnvironmentVariable("DATA_PATH")
                       ?? Path.Combine(Directory.GetCurrentDirectory(), "data");
        _secretsDirectory = Path.Combine(dataPath, ".secrets");

        // 确保目录存在
        if (!Directory.Exists(_secretsDirectory))
        {
            Directory.CreateDirectory(_secretsDirectory);
            _logger.LogInformation("创建密钥存储目录: {Path}", _secretsDirectory);

            // 设置目录权限（仅限 Unix 系统）
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(_secretsDirectory,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "设置密钥目录权限失败");
                }
            }
        }

        _keyStorePath = Path.Combine(_secretsDirectory, "jwt_keys.json");
    }

    /// <summary>
    /// 获取当前主密钥（用于签发Token）
    /// </summary>
    public JwtKeyVersion GetCurrentKey()
    {
        var keyStore = LoadKeyStore();
        var currentKey = keyStore.Keys.FirstOrDefault(k => k.Id == keyStore.CurrentKeyId);

        if (currentKey == null)
        {
            _logger.LogWarning("未找到当前主密钥，正在初始化...");
            return InitializeKeyStore();
        }

        return currentKey;
    }

    /// <summary>
    /// 获取所有有效的验证密钥（包括当前密钥和过渡期密钥）
    /// </summary>
    public List<JwtKeyVersion> GetValidationKeys()
    {
        var keyStore = LoadKeyStore();
        var now = DateTime.UtcNow;

        // 返回所有活跃状态和未过期的密钥
        return keyStore.Keys
            .Where(k => k.Status == JwtKeyStatus.Active || k.Status == JwtKeyStatus.VerifyOnly)
            .Where(k => k.ExpiresAt == null || k.ExpiresAt > now)
            .OrderByDescending(k => k.CreatedAt)
            .ToList();
    }

    /// <summary>
    /// 轮换密钥（生成新密钥，将旧密钥标记为过渡期）
    /// </summary>
    public async Task<JwtKeyVersion> RotateKeyAsync(int transitionPeriodDays = 7)
    {
        await _fileLock.WaitAsync();
        try
        {
            _logger.LogWarning("========================================");
            _logger.LogWarning("开始JWT密钥轮换...");

            var keyStore = LoadKeyStore();
            var now = DateTime.UtcNow;

            // 获取旧的主密钥
            var oldKey = keyStore.Keys.FirstOrDefault(k => k.Id == keyStore.CurrentKeyId);
            if (oldKey != null)
            {
                // 将旧密钥标记为仅验证状态，设置过渡期
                oldKey.Status = JwtKeyStatus.VerifyOnly;
                oldKey.ExpiresAt = now.AddDays(transitionPeriodDays);
                _logger.LogInformation("旧密钥 {KeyId} 已标记为过渡期，将在 {ExpiresAt} 过期",
                    oldKey.Id, oldKey.ExpiresAt);
            }

            // 生成新密钥
            var newKeyId = GenerateKeyId(keyStore.Keys);
            var newKey = new JwtKeyVersion
            {
                Id = newKeyId,
                Key = GenerateSecureKey(64),
                CreatedAt = now,
                ExpiresAt = null, // 主密钥永不过期
                Status = JwtKeyStatus.Active
            };

            // 添加新密钥并设置为当前主密钥
            keyStore.Keys.Add(newKey);
            keyStore.CurrentKeyId = newKeyId;
            keyStore.LastRotationAt = now;

            // 清理过期密钥（保留最近30天内的所有密钥）
            var cleanupDate = now.AddDays(-30);
            keyStore.Keys.RemoveAll(k =>
                k.Status == JwtKeyStatus.Expired &&
                k.ExpiresAt.HasValue &&
                k.ExpiresAt.Value < cleanupDate);

            // 保存密钥存储
            await SaveKeyStoreAsync(keyStore);

            _logger.LogWarning("新密钥 {KeyId} 已生成并激活", newKeyId);
            _logger.LogWarning("过渡期: {Days} 天（旧Token在此期间仍然有效）", transitionPeriodDays);
            _logger.LogWarning("当前活跃密钥数量: {Count}",
                keyStore.Keys.Count(k => k.Status == JwtKeyStatus.Active || k.Status == JwtKeyStatus.VerifyOnly));
            _logger.LogWarning("========================================");

            return newKey;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 撤销指定密钥（紧急撤销）
    /// </summary>
    public async Task RevokeKeyAsync(string keyId)
    {
        await _fileLock.WaitAsync();
        try
        {
            _logger.LogWarning("撤销密钥: {KeyId}", keyId);

            var keyStore = LoadKeyStore();
            var key = keyStore.Keys.FirstOrDefault(k => k.Id == keyId);

            if (key == null)
            {
                throw new InvalidOperationException($"密钥 {keyId} 不存在");
            }

            // 如果撤销的是当前主密钥，需要先轮换
            if (keyStore.CurrentKeyId == keyId)
            {
                _logger.LogWarning("撤销当前主密钥，将自动生成新密钥");
                await RotateKeyAsync(0); // 立即过期，无过渡期

                // 重新加载以获取最新状态
                keyStore = LoadKeyStore();
                key = keyStore.Keys.FirstOrDefault(k => k.Id == keyId);
                if (key == null) return;
            }

            key.Status = JwtKeyStatus.Revoked;
            key.ExpiresAt = DateTime.UtcNow; // 立即过期

            await SaveKeyStoreAsync(keyStore);

            _logger.LogWarning("密钥 {KeyId} 已撤销", keyId);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 清理过期密钥
    /// </summary>
    public async Task CleanupExpiredKeysAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var keyStore = LoadKeyStore();
            var now = DateTime.UtcNow;

            // 将过期的VerifyOnly密钥标记为Expired
            var expiredKeys = keyStore.Keys
                .Where(k => k.Status == JwtKeyStatus.VerifyOnly)
                .Where(k => k.ExpiresAt.HasValue && k.ExpiresAt.Value <= now)
                .ToList();

            foreach (var key in expiredKeys)
            {
                key.Status = JwtKeyStatus.Expired;
                _logger.LogInformation("密钥 {KeyId} 已过期", key.Id);
            }

            // 删除过期超过30天的密钥
            var deleteDate = now.AddDays(-30);
            var removedCount = keyStore.Keys.RemoveAll(k =>
                k.Status == JwtKeyStatus.Expired &&
                k.ExpiresAt.HasValue &&
                k.ExpiresAt.Value < deleteDate);

            if (removedCount > 0 || expiredKeys.Any())
            {
                await SaveKeyStoreAsync(keyStore);
                _logger.LogInformation("清理完成：{Expired} 个密钥已过期，{Deleted} 个旧密钥已删除",
                    expiredKeys.Count, removedCount);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 获取密钥存储信息
    /// </summary>
    public JwtKeyStore GetKeyStore()
    {
        return LoadKeyStore();
    }

    /// <summary>
    /// 检查是否需要轮换（基于时间策略）
    /// </summary>
    public bool ShouldRotate(int rotationDays = 30)
    {
        var keyStore = LoadKeyStore();

        if (keyStore.LastRotationAt == null)
        {
            return true; // 从未轮换过
        }

        var nextRotationDate = keyStore.LastRotationAt.Value.AddDays(rotationDays);
        return DateTime.UtcNow >= nextRotationDate;
    }

    /// <summary>
    /// 加载密钥存储（带缓存）
    /// </summary>
    private JwtKeyStore LoadKeyStore()
    {
        // 使用缓存避免频繁读文件
        if (_cachedKeyStore != null && DateTime.UtcNow - _lastCacheTime < CacheExpiration)
        {
            return _cachedKeyStore;
        }

        // 检查环境变量（优先级最高）
        var envKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            _logger.LogInformation("使用环境变量中的JWT密钥（单密钥模式）");
            var keyStore = new JwtKeyStore
            {
                CurrentKeyId = "env",
                Keys = new List<JwtKeyVersion>
                {
                    new JwtKeyVersion
                    {
                        Id = "env",
                        Key = envKey,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = null,
                        Status = JwtKeyStatus.Active
                    }
                }
            };
            _cachedKeyStore = keyStore;
            _lastCacheTime = DateTime.UtcNow;
            return keyStore;
        }

        // 从文件加载
        if (File.Exists(_keyStorePath))
        {
            try
            {
                var json = File.ReadAllText(_keyStorePath);
                var keyStore = JsonSerializer.Deserialize<JwtKeyStore>(json);
                if (keyStore != null && keyStore.Keys.Any())
                {
                    _logger.LogDebug("从文件加载密钥存储: {Path}", _keyStorePath);
                    _cachedKeyStore = keyStore;
                    _lastCacheTime = DateTime.UtcNow;
                    return keyStore;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取密钥存储文件失败");
            }
        }

        // 初始化新的密钥存储
        _logger.LogWarning("未找到密钥存储，正在初始化...");
        var newKey = InitializeKeyStore();
        var newStore = new JwtKeyStore
        {
            CurrentKeyId = newKey.Id,
            Keys = new List<JwtKeyVersion> { newKey },
            LastRotationAt = DateTime.UtcNow
        };

        SaveKeyStoreAsync(newStore).Wait();
        _cachedKeyStore = newStore;
        _lastCacheTime = DateTime.UtcNow;
        return newStore;
    }

    /// <summary>
    /// 保存密钥存储到文件
    /// </summary>
    private async Task SaveKeyStoreAsync(JwtKeyStore keyStore)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(keyStore, options);
            await File.WriteAllTextAsync(_keyStorePath, json);

            // 设置文件权限（仅限 Unix 系统）
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(_keyStorePath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "设置密钥文件权限失败");
                }
            }

            // 清除缓存，下次读取时重新加载
            _cachedKeyStore = null;

            _logger.LogDebug("密钥存储已保存: {Path}", _keyStorePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存密钥存储失败");
            throw;
        }
    }

    /// <summary>
    /// 初始化密钥存储（创建第一个密钥或从现有jwt.key导入）
    /// </summary>
    private JwtKeyVersion InitializeKeyStore()
    {
        _logger.LogWarning("初始化JWT密钥存储...");

        var now = DateTime.UtcNow;
        var initialKey = new JwtKeyVersion
        {
            Id = "v1",
            CreatedAt = now,
            ExpiresAt = null,
            Status = JwtKeyStatus.Active
        };

        // 尝试从现有的 jwt.key 文件导入密钥
        var oldJwtKeyPath = Path.Combine(_secretsDirectory, "jwt.key");
        if (File.Exists(oldJwtKeyPath))
        {
            try
            {
                var existingKey = File.ReadAllText(oldJwtKeyPath);
                _logger.LogInformation("从现有 jwt.key 文件导入密钥");
                initialKey.Key = existingKey;
                _logger.LogWarning("初始密钥 v1 已从 jwt.key 导入");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "从 jwt.key 导入密钥失败，将生成新密钥");
                initialKey.Key = GenerateSecureKey(64);
                _logger.LogWarning("初始密钥 v1 已生成");
            }
        }
        else
        {
            initialKey.Key = GenerateSecureKey(64);
            _logger.LogWarning("初始密钥 v1 已生成");
        }

        return initialKey;
    }

    /// <summary>
    /// 生成密钥ID（递增版本号）
    /// </summary>
    private string GenerateKeyId(List<JwtKeyVersion> existingKeys)
    {
        var maxVersion = existingKeys
            .Select(k => k.Id)
            .Where(id => id.StartsWith("v") && int.TryParse(id.Substring(1), out _))
            .Select(id => int.Parse(id.Substring(1)))
            .DefaultIfEmpty(0)
            .Max();

        return $"v{maxVersion + 1}";
    }

    /// <summary>
    /// 生成安全的随机密钥（Base64编码）
    /// </summary>
    private string GenerateSecureKey(int length)
    {
        if (length < 32)
        {
            length = 32; // 至少256位
        }

        var key = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }
}
