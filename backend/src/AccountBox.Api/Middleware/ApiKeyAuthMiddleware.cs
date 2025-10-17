using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Api.Middleware;

/// <summary>
/// API密钥认证中间件
/// 从请求头中提取API密钥，验证有效性，并将API密钥信息存入HttpContext
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthMiddleware(RequestDelegate next, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AccountBoxDbContext dbContext)
    {
        // 检查请求头中是否包含API密钥
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new
                {
                    code = "API_KEY_MISSING",
                    message = "API密钥缺失。请在请求头中提供 X-API-Key。"
                }
            });
            return;
        }

        var apiKeyValue = extractedApiKey.ToString();

        // 从数据库中查找匹配的API密钥（通过明文匹配）
        var apiKey = await dbContext.ApiKeys
            .Include(k => k.ApiKeyWebsiteScopes)
            .FirstOrDefaultAsync(k => k.KeyPlaintext == apiKeyValue);

        if (apiKey == null)
        {
            _logger.LogWarning("Invalid API key attempt from {IpAddress} to {Path}",
                context.Connection.RemoteIpAddress,
                context.Request.Path);

            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new
                {
                    code = "API_KEY_INVALID",
                    message = "无效的API密钥。"
                }
            });
            return;
        }

        // TODO: Phase 6+ - 需要实现 Vault 解锁状态检查
        // ⚠️ 限制：当前版本的外部 API 无法访问加密数据（如账号密码）
        // 原因：没有实现 Vault 解锁机制，无法获取 VaultKey
        // 未来改进方案：
        // - Option A: 要求外部 API 调用时提供 Session ID（用户先在 Web UI 解锁）
        // - Option B: 为 API Key 实现独立的解锁机制
        // - Option C: 允许外部 API 仅访问未加密数据

        // 记录API调用日志
        _logger.LogInformation("API Key {ApiKeyId} ({ApiKeyName}) accessed {Method} {Path} from {IpAddress} at {Timestamp}",
            apiKey.Id,
            apiKey.Name,
            context.Request.Method,
            context.Request.Path,
            context.Connection.RemoteIpAddress,
            DateTime.UtcNow);

        // 更新最后使用时间
        apiKey.LastUsedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // 将API密钥信息存入HttpContext，供后续使用
        context.Items["ApiKey"] = apiKey;

        // 继续处理请求
        await _next(context);
    }
}
