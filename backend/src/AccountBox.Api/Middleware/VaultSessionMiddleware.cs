using AccountBox.Api.Services;
using AccountBox.Core.Models;
using System.Text.Json;

namespace AccountBox.Api.Middleware;

/// <summary>
/// Vault 会话验证中间件
/// 验证 X-Vault-Session 头，确保 Vault 已解锁
/// </summary>
public class VaultSessionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<VaultSessionMiddleware> _logger;

    // 无需验证的端点（白名单）
    private static readonly HashSet<string> _publicEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/vault/status",
        "/api/vault/initialize",
        "/api/vault/unlock",
        "/swagger",
        "/health"
    };

    public VaultSessionMiddleware(
        RequestDelegate next,
        ILogger<VaultSessionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, VaultService vaultService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // 检查是否是公开端点
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        // 检查 X-Vault-Session 头
        var sessionId = context.Request.Headers["X-Vault-Session"].ToString();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            _logger.LogWarning("Missing X-Vault-Session header for protected endpoint: {Path}", path);
            await WriteUnauthorizedResponse(context, "MISSING_SESSION", "X-Vault-Session header is required");
            return;
        }

        // 验证会话并获取 VaultKey
        var vaultKey = vaultService.GetVaultKey(sessionId);

        if (vaultKey == null)
        {
            _logger.LogWarning("Invalid or expired session {SessionId} for endpoint: {Path}", sessionId, path);
            await WriteUnauthorizedResponse(context, "INVALID_SESSION", "Session is invalid or expired. Please unlock the vault.");
            return;
        }

        // 会话有效，继续处理请求
        _logger.LogDebug("Valid session {SessionId} for endpoint: {Path}", sessionId, path);

        // 将 VaultKey 存储到 HttpContext.Items 中，供后续服务使用
        context.Items["VaultKey"] = vaultKey;
        context.Items["VaultSessionId"] = sessionId;

        await _next(context);
    }

    /// <summary>
    /// 检查是否是公开端点
    /// </summary>
    private static bool IsPublicEndpoint(string path)
    {
        // 精确匹配或前缀匹配
        return _publicEndpoints.Any(endpoint =>
            path.Equals(endpoint, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 返回 401 Unauthorized 响应
    /// </summary>
    private static async Task WriteUnauthorizedResponse(
        HttpContext context,
        string errorCode,
        string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(errorCode, message);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
