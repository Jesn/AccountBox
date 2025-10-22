using AccountBox.Core.Enums;
using AccountBox.Data.Entities;

namespace AccountBox.Data.Repositories;

/// <summary>
/// Account 仓储接口
/// 定义账号的 CRUD 操作、分页查询和软删除支持
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// 获取分页账号列表（按用户名字母序）
    /// 只返回活跃账号（不包括回收站）
    /// </summary>
    Task<(List<Account> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId = null,
        string? searchTerm = null,
        string? status = null);

    /// <summary>
    /// 根据 ID 获取账号
    /// 注意:包括软删除的账号,以便查看回收站中的项目详情
    /// </summary>
    Task<Account?> GetByIdAsync(int id);

    /// <summary>
    /// 创建账号
    /// </summary>
    Task<Account> CreateAsync(Account account);

    /// <summary>
    /// 更新账号
    /// </summary>
    Task UpdateAsync(Account account);

    /// <summary>
    /// 软删除账号（移入回收站）
    /// </summary>
    Task SoftDeleteAsync(int id);

    /// <summary>
    /// 恢复账号（从回收站恢复）
    /// </summary>
    Task RestoreAsync(int id);

    /// <summary>
    /// 永久删除账号（从数据库物理删除）
    /// </summary>
    Task PermanentlyDeleteAsync(int id);

    /// <summary>
    /// 检查账号是否存在
    /// </summary>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 获取回收站中的账号列表（分页）
    /// </summary>
    Task<(List<Account> Items, int TotalCount)> GetDeletedPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId = null,
        string? searchTerm = null);

    /// <summary>
    /// 清空回收站（永久删除所有已删除的账号）
    /// </summary>
    Task EmptyRecycleBinAsync(int? websiteId = null);

    /// <summary>
    /// 检查指定网站下是否存在指定用户名的账号
    /// </summary>
    /// <param name="websiteId">网站ID</param>
    /// <param name="username">用户名</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    Task<bool> UsernameExistsAsync(int websiteId, string username);
}
