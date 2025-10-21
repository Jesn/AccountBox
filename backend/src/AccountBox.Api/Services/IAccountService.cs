using AccountBox.Core.Models;
using AccountBox.Core.Models.Account;

namespace AccountBox.Api.Services;

/// <summary>
/// Account 业务服务接口
/// 管理账号的 CRUD 操作（明文存储，适用于个人自托管场景）
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// 获取分页账号列表（只返回活跃账号）
    /// </summary>
    Task<PagedResult<AccountResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? websiteId,
        string? searchTerm = null,
        string? status = null);

    /// <summary>
    /// 根据 ID 获取账号
    /// </summary>
    Task<AccountResponse?> GetByIdAsync(int id);

    /// <summary>
    /// 创建账号
    /// </summary>
    Task<AccountResponse> CreateAsync(CreateAccountRequest request);

    /// <summary>
    /// 更新账号
    /// </summary>
    Task<AccountResponse> UpdateAsync(int id, UpdateAccountRequest request);

    /// <summary>
    /// 软删除账号（移入回收站）
    /// </summary>
    Task SoftDeleteAsync(int id);

    /// <summary>
    /// 启用账号
    /// </summary>
    Task EnableAccountAsync(int id);

    /// <summary>
    /// 禁用账号
    /// </summary>
    Task DisableAccountAsync(int id);

    /// <summary>
    /// 检查指定网站下是否存在指定用户名的账号
    /// </summary>
    /// <param name="websiteId">网站ID</param>
    /// <param name="username">用户名</param>
    /// <returns>如果存在返回true，否则返回false</returns>
    Task<bool> UsernameExistsAsync(int websiteId, string username);
}
