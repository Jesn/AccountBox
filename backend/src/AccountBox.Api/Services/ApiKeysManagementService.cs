using AccountBox.Core.Enums;
using AccountBox.Core.Models;
using AccountBox.Core.Models.ApiKey;
using AccountBox.Core.Services;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Api.Services;

/// <summary>
/// API密钥管理服务（明文存储模式，无需 Vault）
/// </summary>
public class ApiKeysManagementService : IApiKeysManagementService
{
    private readonly AccountBoxDbContext _context;
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysManagementService(
        AccountBoxDbContext context,
        IApiKeyService apiKeyService)
    {
        _context = context;
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// 获取所有API密钥
    /// </summary>
    public async Task<List<ApiKeyDto>> GetAllAsync()
    {
        var apiKeys = await _context.ApiKeys
            .Include(k => k.ApiKeyWebsiteScopes)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();

        return apiKeys.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 创建新的API密钥
    /// </summary>
    public async Task<ApiKeyDto> CreateAsync(CreateApiKeyRequest request)
    {
        // 验证输入
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("密钥名称不能为空");

        var scopeType = Enum.Parse<ApiKeyScopeType>(request.ScopeType, ignoreCase: true);

        if (scopeType == ApiKeyScopeType.Specific && (request.WebsiteIds == null || !request.WebsiteIds.Any()))
            throw new ArgumentException("指定网站作用域时必须选择至少一个网站");

        // 生成密钥
        var keyPlaintext = _apiKeyService.GenerateApiKey();
        var keyHash = _apiKeyService.HashApiKey(keyPlaintext);

        // 创建实体
        var apiKey = new ApiKey
        {
            Name = request.Name,
            KeyPlaintext = keyPlaintext,
            KeyHash = keyHash,
            ScopeType = scopeType,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApiKeys.Add(apiKey);
        await _context.SaveChangesAsync();

        // 添加网站作用域
        if (scopeType == ApiKeyScopeType.Specific && request.WebsiteIds != null)
        {
            foreach (var websiteId in request.WebsiteIds)
            {
                _context.ApiKeyWebsiteScopes.Add(new ApiKeyWebsiteScope
                {
                    ApiKeyId = apiKey.Id,
                    WebsiteId = websiteId
                });
            }
            await _context.SaveChangesAsync();

            // 重新加载以包含作用域
            await _context.Entry(apiKey).Collection(k => k.ApiKeyWebsiteScopes).LoadAsync();
        }

        // 映射并返回（包含明文密钥）
        var dto = MapToDto(apiKey);
        dto.KeyPlaintext = keyPlaintext; // 仅在创建时返回明文

        return dto;
    }

    /// <summary>
    /// 删除API密钥
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var apiKey = await _context.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id);

        if (apiKey == null)
            throw new KeyNotFoundException($"API密钥 {id} 不存在");

        _context.ApiKeys.Remove(apiKey);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 映射实体到DTO
    /// </summary>
    private static ApiKeyDto MapToDto(ApiKey apiKey)
    {
        return new ApiKeyDto
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            KeyPlaintext = apiKey.KeyPlaintext, // 始终包含明文（个人工具场景）
            ScopeType = apiKey.ScopeType.ToString(),
            WebsiteIds = apiKey.ApiKeyWebsiteScopes.Select(s => s.WebsiteId).ToList(),
            CreatedAt = apiKey.CreatedAt,
            LastUsedAt = apiKey.LastUsedAt
        };
    }
}
