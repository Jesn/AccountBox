using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using AccountBox.Api.Services;
using AccountBox.Core.Models;

namespace AccountBox.Api.Middleware;

/// <summary>
/// 全局异常处理中间件
/// 捕获所有未处理的异常并返回标准化的错误响应
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "MISSING_ARGUMENT", exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", exception.Message),
            CryptographicException => (HttpStatusCode.Unauthorized, "AUTHENTICATION_FAILED", "主密码错误或数据已损坏"),
            InvalidOperationException => (HttpStatusCode.Conflict, "INVALID_OPERATION", exception.Message),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "发生了意外错误")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(
            errorCode,
            message,
            _env.IsDevelopment() ? exception.StackTrace : null // 仅在开发环境返回堆栈跟踪
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
