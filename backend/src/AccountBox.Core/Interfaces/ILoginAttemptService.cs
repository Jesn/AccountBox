namespace AccountBox.Core.Interfaces;

/// <summary>
/// 登录尝试记录服务接口
/// </summary>
public interface ILoginAttemptService
{
    /// <summary>
    /// 记录登录尝试（成功或失败）
    /// </summary>
    Task RecordLoginAttemptAsync(string ipAddress, string? userAgent, bool isSuccessful, string? failureReason = null);
}