using AccountBox.Core.Models.ApiKey;

namespace AccountBox.Api.Services;

/// <summary>
/// API密钥管理服务接口（明文存储模式，无需 Vault）
/// </summary>
public interface IApiKeysManagementService
{
    /// <summary>
    /// 获取所有API密钥
    /// </summary>
    Task<List<ApiKeyDto>> GetAllAsync();

    /// <summary>
    /// 创建新的API密钥
    /// </summary>
    Task<ApiKeyDto> CreateAsync(CreateApiKeyRequest request);

    /// <summary>
    /// 删除API密钥
    /// </summary>
    Task DeleteAsync(int id);
}
