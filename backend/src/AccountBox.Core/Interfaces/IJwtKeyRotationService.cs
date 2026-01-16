using AccountBox.Core.Models.Auth;

namespace AccountBox.Core.Interfaces;

/// <summary>
/// JWT密钥轮换服务接口
/// </summary>
public interface IJwtKeyRotationService
{
    /// <summary>
    /// 获取当前主密钥（用于签发Token）
    /// </summary>
    JwtKeyVersion GetCurrentKey();

    /// <summary>
    /// 获取所有有效的验证密钥（包括当前密钥和过渡期密钥）
    /// </summary>
    List<JwtKeyVersion> GetValidationKeys();

    /// <summary>
    /// 轮换密钥（生成新密钥，将旧密钥标记为过渡期）
    /// </summary>
    /// <param name="transitionPeriodDays">过渡期天数（默认7天）</param>
    /// <returns>新密钥版本</returns>
    Task<JwtKeyVersion> RotateKeyAsync(int transitionPeriodDays = 7);

    /// <summary>
    /// 撤销指定密钥（紧急撤销）
    /// </summary>
    Task RevokeKeyAsync(string keyId);

    /// <summary>
    /// 清理过期密钥
    /// </summary>
    Task CleanupExpiredKeysAsync();

    /// <summary>
    /// 获取密钥存储信息
    /// </summary>
    JwtKeyStore GetKeyStore();

    /// <summary>
    /// 检查是否需要轮换（基于时间策略）
    /// </summary>
    bool ShouldRotate(int rotationDays = 30);
}
