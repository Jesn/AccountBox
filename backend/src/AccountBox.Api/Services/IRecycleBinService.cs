using AccountBox.Core.Models;
using AccountBox.Core.Models.RecycleBin;

namespace AccountBox.Api.Services;

/// <summary>
/// 回收站业务服务接口
/// 管理已删除账号的查看、恢复和永久删除（明文存储模式）
/// </summary>
public interface IRecycleBinService
{
    /// <summary>
    /// 获取回收站中的分页账号列表
    /// </summary>
    Task<PagedResult<DeletedAccountResponse>> GetDeletedAccountsAsync(
        int pageNumber,
        int pageSize,
        int? websiteId);

    /// <summary>
    /// 恢复账号（从回收站恢复到活跃状态）
    /// </summary>
    Task RestoreAccountAsync(int accountId);

    /// <summary>
    /// 永久删除账号（从数据库物理删除）
    /// </summary>
    Task PermanentlyDeleteAccountAsync(int accountId);

    /// <summary>
    /// 清空回收站（永久删除所有已删除的账号）
    /// </summary>
    Task EmptyRecycleBinAsync(int? websiteId = null);
}
