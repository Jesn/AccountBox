namespace AccountBox.Core.Models.Vault;

/// <summary>
/// 初始化 Vault 请求
/// </summary>
public class InitializeVaultRequest
{
    /// <summary>
    /// 主密码
    /// </summary>
    public string MasterPassword { get; set; } = null!;
}

/// <summary>
/// 解锁 Vault 请求
/// </summary>
public class UnlockVaultRequest
{
    /// <summary>
    /// 主密码
    /// </summary>
    public string MasterPassword { get; set; } = null!;
}

/// <summary>
/// 修改主密码请求
/// </summary>
public class ChangeMasterPasswordRequest
{
    /// <summary>
    /// 旧主密码
    /// </summary>
    public string OldMasterPassword { get; set; } = null!;

    /// <summary>
    /// 新主密码
    /// </summary>
    public string NewMasterPassword { get; set; } = null!;
}

/// <summary>
/// Vault 会话响应
/// </summary>
public class VaultSessionResponse
{
    /// <summary>
    /// 会话ID（用于后续API请求的认证）
    /// </summary>
    public string SessionId { get; set; } = null!;

    /// <summary>
    /// 会话过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Vault 状态响应
/// </summary>
public class VaultStatusResponse
{
    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// 是否已解锁
    /// </summary>
    public bool IsUnlocked { get; set; }
}
