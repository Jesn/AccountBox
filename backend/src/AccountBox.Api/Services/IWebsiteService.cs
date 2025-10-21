using AccountBox.Core.Models;
using AccountBox.Core.Models.Website;

namespace AccountBox.Api.Services;

/// <summary>
/// Website 业务服务接口
/// 管理网站的 CRUD 操作、分页和业务验证
/// </summary>
public interface IWebsiteService
{
    /// <summary>
    /// 获取分页网站列表
    /// </summary>
    Task<PagedResult<WebsiteResponse>> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// 根据 ID 获取网站
    /// </summary>
    Task<WebsiteResponse?> GetByIdAsync(int id);

    /// <summary>
    /// 创建网站
    /// </summary>
    Task<WebsiteResponse> CreateAsync(CreateWebsiteRequest request);

    /// <summary>
    /// 更新网站
    /// </summary>
    Task<WebsiteResponse> UpdateAsync(int id, UpdateWebsiteRequest request);

    /// <summary>
    /// 删除网站（级联删除保护）
    /// </summary>
    Task DeleteAsync(int id, bool confirmed = false);

    /// <summary>
    /// 获取网站的账号统计
    /// </summary>
    Task<AccountCountResponse> GetAccountCountAsync(int id);
}
