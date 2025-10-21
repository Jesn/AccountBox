using AccountBox.Core.Models.Account;

namespace AccountBox.Api.Services;

/// <summary>
/// 随机账号服务接口
/// 提供随机选择启用账号的功能（适用于外部API调用场景，如爬虫轮询）
/// 带24小时缓存防重复功能
/// </summary>
public interface IRandomAccountService
{
    /// <summary>
    /// 从指定网站随机获取一个启用状态的账号（带24小时缓存防重复）
    /// </summary>
    /// <param name="apiKeyId">API密钥ID（用于缓存key）</param>
    /// <param name="websiteId">网站ID</param>
    /// <returns>随机选中的账号响应，如果没有可用账号则返回null</returns>
    Task<AccountResponse?> GetRandomEnabledAccountAsync(int apiKeyId, int websiteId);
}
