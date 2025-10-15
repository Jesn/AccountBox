using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Vault;
using AccountBox.Data.Entities;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Services;

/// <summary>
/// Vault 业务服务
/// 管理应用初始化、解锁、锁定和主密码修改
/// </summary>
public class VaultService
{
    private readonly IVaultManager _vaultManager;
    private readonly KeySlotRepository _keySlotRepository;

    // 内存中的会话存储（生产环境应使用 Redis 等分布式缓存）
    private static readonly Dictionary<string, VaultSession> _sessions = new();
    private static readonly object _sessionLock = new();

    // 密码重试限制（防止暴力破解）
    private static readonly Dictionary<string, FailedAttempt> _failedAttempts = new();
    private static readonly object _attemptLock = new();
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);

    public VaultService(
        IVaultManager vaultManager,
        KeySlotRepository keySlotRepository)
    {
        _vaultManager = vaultManager ?? throw new ArgumentNullException(nameof(vaultManager));
        _keySlotRepository = keySlotRepository ?? throw new ArgumentNullException(nameof(keySlotRepository));
    }

    /// <summary>
    /// 获取 Vault 状态
    /// </summary>
    public async Task<VaultStatusResponse> GetStatusAsync()
    {
        var isInitialized = await _keySlotRepository.ExistsAsync();
        // 这里简化处理，实际应该检查是否有有效的会话
        var isUnlocked = false;

        return new VaultStatusResponse
        {
            IsInitialized = isInitialized,
            IsUnlocked = isUnlocked
        };
    }

    /// <summary>
    /// 初始化 Vault（首次设置主密码）
    /// </summary>
    public async Task<VaultSessionResponse> InitializeAsync(InitializeVaultRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.MasterPassword))
        {
            throw new ArgumentException("Master password cannot be empty", nameof(request));
        }

        // 检查是否已初始化
        if (await _keySlotRepository.ExistsAsync())
        {
            throw new InvalidOperationException("Vault is already initialized");
        }

        // 使用 VaultManager 初始化
        var (encryptedVaultKey, vaultKeyIV, vaultKeyTag, argon2Salt,
             argon2Iterations, argon2MemorySize, argon2Parallelism) =
            _vaultManager.Initialize(request.MasterPassword);

        // 创建 KeySlot 实体
        var keySlot = new KeySlot
        {
            EncryptedVaultKey = encryptedVaultKey,
            VaultKeyIV = vaultKeyIV,
            VaultKeyTag = vaultKeyTag,
            Argon2Salt = argon2Salt,
            Argon2Iterations = argon2Iterations,
            Argon2MemorySize = argon2MemorySize,
            Argon2Parallelism = argon2Parallelism
        };

        await _keySlotRepository.CreateAsync(keySlot);

        // 创建会话
        var session = CreateSession(encryptedVaultKey);

        return new VaultSessionResponse
        {
            SessionId = session.SessionId,
            ExpiresAt = session.ExpiresAt
        };
    }

    /// <summary>
    /// 解锁 Vault
    /// </summary>
    public async Task<VaultSessionResponse> UnlockAsync(UnlockVaultRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.MasterPassword))
        {
            throw new ArgumentException("Master password cannot be empty", nameof(request));
        }

        // 检查是否被锁定
        CheckIfLockedOut();

        // 获取 KeySlot
        var keySlot = await _keySlotRepository.GetAsync();
        if (keySlot == null)
        {
            throw new InvalidOperationException("Vault is not initialized");
        }

        try
        {
            // 使用 VaultManager 解锁
            var vaultKey = _vaultManager.Unlock(
                request.MasterPassword,
                keySlot.EncryptedVaultKey,
                keySlot.VaultKeyIV,
                keySlot.VaultKeyTag,
                keySlot.Argon2Salt,
                keySlot.Argon2Iterations,
                keySlot.Argon2MemorySize,
                keySlot.Argon2Parallelism);

            // 解锁成功，清除失败记录
            ClearFailedAttempts();

            // 创建会话
            var session = CreateSession(vaultKey);

            return new VaultSessionResponse
            {
                SessionId = session.SessionId,
                ExpiresAt = session.ExpiresAt
            };
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // 解锁失败，记录失败次数
            RecordFailedAttempt();
            throw; // 重新抛出异常
        }
    }

    /// <summary>
    /// 锁定 Vault
    /// </summary>
    public Task LockAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
        }

        lock (_sessionLock)
        {
            _sessions.Remove(sessionId);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 修改主密码
    /// </summary>
    public async Task ChangeMasterPasswordAsync(ChangeMasterPasswordRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OldMasterPassword))
        {
            throw new ArgumentException("Old master password cannot be empty", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.NewMasterPassword))
        {
            throw new ArgumentException("New master password cannot be empty", nameof(request));
        }

        // 获取 KeySlot
        var keySlot = await _keySlotRepository.GetAsync();
        if (keySlot == null)
        {
            throw new InvalidOperationException("Vault is not initialized");
        }

        // 使用 VaultManager 修改密码
        var (newEncryptedVaultKey, newVaultKeyIV, newVaultKeyTag, newArgon2Salt,
             newArgon2Iterations, newArgon2MemorySize, newArgon2Parallelism) =
            _vaultManager.ChangeMasterPassword(
                request.OldMasterPassword,
                request.NewMasterPassword,
                keySlot.EncryptedVaultKey,
                keySlot.VaultKeyIV,
                keySlot.VaultKeyTag,
                keySlot.Argon2Salt,
                keySlot.Argon2Iterations,
                keySlot.Argon2MemorySize,
                keySlot.Argon2Parallelism);

        // 更新 KeySlot
        keySlot.EncryptedVaultKey = newEncryptedVaultKey;
        keySlot.VaultKeyIV = newVaultKeyIV;
        keySlot.VaultKeyTag = newVaultKeyTag;
        keySlot.Argon2Salt = newArgon2Salt;
        keySlot.Argon2Iterations = newArgon2Iterations;
        keySlot.Argon2MemorySize = newArgon2MemorySize;
        keySlot.Argon2Parallelism = newArgon2Parallelism;

        await _keySlotRepository.UpdateAsync(keySlot);

        // 清除所有会话（强制重新登录）
        lock (_sessionLock)
        {
            _sessions.Clear();
        }
    }

    /// <summary>
    /// 获取会话的 VaultKey
    /// </summary>
    public byte[]? GetVaultKey(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        lock (_sessionLock)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (session.ExpiresAt > DateTime.UtcNow)
                {
                    return session.VaultKey;
                }
                else
                {
                    // 会话过期，清除
                    _sessions.Remove(sessionId);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 创建会话
    /// </summary>
    private VaultSession CreateSession(byte[] vaultKey)
    {
        var sessionId = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(8); // 8小时会话

        var session = new VaultSession
        {
            SessionId = sessionId,
            VaultKey = vaultKey,
            ExpiresAt = expiresAt
        };

        lock (_sessionLock)
        {
            _sessions[sessionId] = session;
        }

        return session;
    }

    /// <summary>
    /// 内部会话类
    /// </summary>
    private class VaultSession
    {
        public string SessionId { get; set; } = null!;
        public byte[] VaultKey { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// 失败尝试记录
    /// </summary>
    private class FailedAttempt
    {
        public int Count { get; set; }
        public DateTime? LockoutUntil { get; set; }
    }

    /// <summary>
    /// 检查是否被锁定
    /// </summary>
    private void CheckIfLockedOut()
    {
        lock (_attemptLock)
        {
            const string key = "unlock"; // 全局锁定键（单用户应用）

            if (_failedAttempts.TryGetValue(key, out var attempt))
            {
                if (attempt.LockoutUntil.HasValue && attempt.LockoutUntil.Value > DateTime.UtcNow)
                {
                    var remainingTime = attempt.LockoutUntil.Value - DateTime.UtcNow;
                    throw new TooManyAttemptsException(
                        $"Too many failed attempts. Please try again in {Math.Ceiling(remainingTime.TotalMinutes)} minutes.",
                        attempt.LockoutUntil.Value);
                }

                // 锁定期已过，清除记录
                if (attempt.LockoutUntil.HasValue && attempt.LockoutUntil.Value <= DateTime.UtcNow)
                {
                    _failedAttempts.Remove(key);
                }
            }
        }
    }

    /// <summary>
    /// 记录失败尝试
    /// </summary>
    private void RecordFailedAttempt()
    {
        lock (_attemptLock)
        {
            const string key = "unlock";

            if (!_failedAttempts.TryGetValue(key, out var attempt))
            {
                attempt = new FailedAttempt { Count = 0 };
                _failedAttempts[key] = attempt;
            }

            attempt.Count++;

            if (attempt.Count >= MaxFailedAttempts)
            {
                attempt.LockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
            }
        }
    }

    /// <summary>
    /// 清除失败尝试记录
    /// </summary>
    private void ClearFailedAttempts()
    {
        lock (_attemptLock)
        {
            const string key = "unlock";
            _failedAttempts.Remove(key);
        }
    }
}

/// <summary>
/// 尝试次数过多异常
/// </summary>
public class TooManyAttemptsException : Exception
{
    public DateTime LockoutUntil { get; }

    public TooManyAttemptsException(string message, DateTime lockoutUntil) : base(message)
    {
        LockoutUntil = lockoutUntil;
    }
}
