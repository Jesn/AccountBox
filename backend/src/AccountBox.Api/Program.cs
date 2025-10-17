using AccountBox.Api.Middleware;
using AccountBox.Api.Services;
using AccountBox.Core.Interfaces;
using AccountBox.Data.DbContext;
using AccountBox.Data.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite 默认端口
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 配置数据库上下文
builder.Services.AddDbContext<AccountBoxDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置依赖注入 - 仓储层
builder.Services.AddScoped<WebsiteRepository>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<SearchRepository>();

// 配置依赖注入 - 业务服务层
builder.Services.AddScoped<WebsiteService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<RecycleBinService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<AccountBox.Core.Services.PasswordGeneratorService>();
builder.Services.AddScoped<AccountBox.Core.Services.IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<ApiKeysManagementService>();
builder.Services.AddScoped<RandomAccountService>();

// HTTP Context Accessor（用于获取当前请求上下文）
builder.Services.AddHttpContextAccessor();

// 配置控制器
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

// 全局异常处理中间件（必须在最前面）
app.UseMiddleware<ExceptionMiddleware>();

// 静态文件服务 (用于 Docker 单镜像部署) - 必须在 CORS 之前
app.UseDefaultFiles(); // 启用默认文档(index.html)
app.UseStaticFiles();  // 提供 wwwroot 下的静态文件

// CORS 中间件
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 路由
app.UseRouting();

// API密钥认证中间件（仅应用于 /api/external/* 路径）
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/external"),
    appBuilder => appBuilder.UseMiddleware<ApiKeyAuthMiddleware>()
);

// 控制器端点
app.MapControllers();

// 健康检查端点
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

// 保留示例端点用于测试
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// SPA fallback 路由 - 所有非 API、非静态文件请求都返回 index.html
app.MapFallbackToFile("index.html");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// 使 Program 类对测试项目可见
public partial class Program { }
