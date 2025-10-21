using AccountBox.Data.Entities;

namespace AccountBox.Data.Repositories;

/// <summary>
/// Website 仓储接口
/// 定义网站的 CRUD 操作、分页查询和账号统计
/// </summary>
public interface IWebsiteRepository
{
    /// <summary>
    /// 获取分页网站列表（按创建时间降序）
    /// </summary>
    Task<(List<Website> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize);

    /// <summary>
    /// 获取所有网站
    /// </summary>
    Task<List<Website>> GetAllAsync();

    /// <summary>
    /// 根据 ID 获取网站
    /// </summary>
    Task<Website?> GetByIdAsync(int id);

    /// <summary>
    /// 根据 ID 列表获取多个网站
    /// </summary>
    Task<List<Website>> GetByIdsAsync(List<int> ids);

    /// <summary>
    /// 创建网站
    /// </summary>
    Task<Website> CreateAsync(Website website);

    /// <summary>
    /// 更新网站
    /// </summary>
    Task UpdateAsync(Website website);

    /// <summary>
    /// 删除网站
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// 检查网站是否存在
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 获取网站下的活跃账号数（不包括回收站）
    /// </summary>
    Task<int> GetActiveAccountCountAsync(int websiteId);

    /// <summary>
    /// 获取网站下的禁用账号数（不包括回收站）
    /// </summary>
    Task<int> GetDisabledAccountCountAsync(int websiteId);

    /// <summary>
    /// 获取网站下的回收站账号数
    /// </summary>
    Task<int> GetDeletedAccountCountAsync(int websiteId);

    /// <summary>
    /// 获取网站下的总账号数（包括回收站）
    /// </summary>
    Task<int> GetTotalAccountCountAsync(int websiteId);
}
