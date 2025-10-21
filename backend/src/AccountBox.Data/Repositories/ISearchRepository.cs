using AccountBox.Data.Entities;

namespace AccountBox.Data.Repositories;

/// <summary>
/// 搜索仓储接口
/// 定义全局账号搜索功能
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// 搜索账号（支持网站名、域名、用户名、标签搜索）
    /// 注意：备注字段是加密的，需要在 Service 层解密后再过滤
    /// </summary>
    Task<(List<Account> Items, int TotalCount)> SearchAsync(
        string query,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// 获取所有账号（用于备注字段的二次过滤）
    /// 注意：此方法仅用于小数据量场景，生产环境应使用 FTS5
    /// </summary>
    Task<List<Account>> GetAllActiveAccountsAsync();
}
