using System.Text;
using AccountBox.Api.Middleware;
using AccountBox.Api.Services;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Auth;
using AccountBox.Data.DbContext;
using AccountBox.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 从配置读取 CORS 允许来源（数组）
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:5093", "http://localhost:5173" };

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins) // 从配置读取允许来源
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 配置数据库上下文（支持 SQLite、PostgreSQL 和 MySQL）
builder.Services.AddDbContext<AccountBoxDbContext>(options =>
{
    var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER")?.ToLower() ?? "sqlite";
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

    if (dbProvider == "postgresql" || dbProvider == "postgres")
    {
        // 使用 PostgreSQL
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.UseNpgsql(connectionString,
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_PostgreSQL", "public")
                          .MigrationsAssembly("AccountBox.Data.Migrations.PostgreSQL"));
        }
        else
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_PostgreSQL", "public")
                          .MigrationsAssembly("AccountBox.Data.Migrations.PostgreSQL"));
        }
    }
    else if (dbProvider == "mysql")
    {
        // 使用 MySQL
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_MySQL")
                          .MigrationsAssembly("AccountBox.Data.Migrations.MySQL"));
        }
        else
        {
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (defaultConnectionString != null)
            {
                options.UseMySql(defaultConnectionString, ServerVersion.AutoDetect(defaultConnectionString),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory_MySQL")
                              .MigrationsAssembly("AccountBox.Data.Migrations.MySQL"));
            }
        }
    }
    else
    {
        // 使用 SQLite（默认）
        var dbPath = Environment.GetEnvironmentVariable("DATABASE_PATH");
        if (!string.IsNullOrWhiteSpace(dbPath))
        {
            options.UseSqlite($"Data Source={dbPath}",
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_Sqlite")
                          .MigrationsAssembly("AccountBox.Data.Migrations.Sqlite"));
        }
        else
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsHistoryTable("__EFMigrationsHistory_Sqlite")
                          .MigrationsAssembly("AccountBox.Data.Migrations.Sqlite"));
        }
    }
});

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

// ========================================
// 密钥管理服务（首次启动自动生成密钥）
// ========================================
// 创建临时的 LoggerFactory 用于初始化 SecretsManager
using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<SecretsManager>();

var secretsManager = new SecretsManager(logger, builder.Configuration);
builder.Services.AddSingleton(secretsManager);

// 获取或生成 JWT 密钥
var jwtSecretKey = secretsManager.GetOrGenerateJwtSecretKey();

// 获取或生成主密码哈希（启动时初始化，确保密码文件存在）
var masterPasswordHash = secretsManager.GetOrGenerateMasterPasswordHash();

// 配置JWT设置（从appsettings.json读取，但密钥从密钥管理器获取）
builder.Services.Configure<JwtSettings>(options =>
{
    var jwtConfig = builder.Configuration.GetSection("JwtSettings");
    options.SecretKey = jwtSecretKey; // 使用动态生成的密钥
    options.Issuer = jwtConfig["Issuer"] ?? "AccountBox";
    options.Audience = jwtConfig["Audience"] ?? "AccountBox-Web";
    options.ExpirationHours = int.Parse(jwtConfig["ExpirationHours"] ?? "24");
    options.ValidateIssuer = bool.Parse(jwtConfig["ValidateIssuer"] ?? "true");
    options.ValidateAudience = bool.Parse(jwtConfig["ValidateAudience"] ?? "true");
    options.ValidateLifetime = bool.Parse(jwtConfig["ValidateLifetime"] ?? "true");
    options.ValidateIssuerSigningKey = bool.Parse(jwtConfig["ValidateIssuerSigningKey"] ?? "true");
});

// 注册JWT服务和MemoryCache
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddMemoryCache();

// 配置JWT认证
var jwtSettings = new JwtSettings
{
    SecretKey = jwtSecretKey,
    Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "AccountBox",
    Audience = builder.Configuration["JwtSettings:Audience"] ?? "AccountBox-Web",
    ExpirationHours = int.Parse(builder.Configuration["JwtSettings:ExpirationHours"] ?? "24"),
    ValidateIssuer = bool.Parse(builder.Configuration["JwtSettings:ValidateIssuer"] ?? "true"),
    ValidateAudience = bool.Parse(builder.Configuration["JwtSettings:ValidateAudience"] ?? "true"),
    ValidateLifetime = bool.Parse(builder.Configuration["JwtSettings:ValidateLifetime"] ?? "true"),
    ValidateIssuerSigningKey = bool.Parse(builder.Configuration["JwtSettings:ValidateIssuerSigningKey"] ?? "true")
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtSettings.ValidateIssuer,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = jwtSettings.ValidateAudience,
        ValidAudience = jwtSettings.Audience,

        ValidateLifetime = jwtSettings.ValidateLifetime,
        ClockSkew = TimeSpan.Zero, // 不允许时钟偏移

        ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// 添加授权服务
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.

// 全局异常处理中间件（必须在最前面）
app.UseMiddleware<ExceptionMiddleware>();

// 静态文件服务 (Docker 单镜像部署可用) - 仅当 wwwroot 存在时启用
var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
PhysicalFileProvider? fileProvider = null;
if (Directory.Exists(webRoot))
{
    fileProvider = new PhysicalFileProvider(webRoot);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider }); // 启用默认文档(index.html)
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });   // 提供 wwwroot 下的静态文件
}

// 应用数据库迁移（自动建表/升级）
// 注意：由于多数据库迁移文件共存的限制，这里使用 EnsureCreated() 而不是 Migrate()
// 生产环境建议使用独立的迁移脚本
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountBoxDbContext>();

    // 检查数据库是否存在，如果不存在则创建
    if (!db.Database.CanConnect() || !db.Database.GetAppliedMigrations().Any())
    {
        // 使用 EnsureCreated 创建数据库（不使用迁移）
        db.Database.EnsureCreated();
    }
}


if (app.Environment.IsDevelopment())
{


    app.UseSwagger();
    app.UseSwaggerUI();
}

// 路由
app.UseRouting();

// CORS 中间件（注意：应在 UseRouting 之后、终结点映射之前）
app.UseCors();

// JWT认证和授权中间件（必须在UseRouting之后，MapControllers之前）
app.UseAuthentication();
app.UseAuthorization();

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

// SPA fallback 路由 - 仅当存在 wwwroot/index.html 时生效（容器内托管静态前端）
// 注意：排除 API 路径，只处理前端路由
if (fileProvider is not null)
{
    app.MapFallback(async context =>
    {
        // 如果是 API 请求，不处理（让 404 正常返回）
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var fileInfo = fileProvider.GetFileInfo("index.html");
        if (fileInfo.Exists)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(fileInfo);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    });
}

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// 使 Program 类对测试项目可见
public partial class Program { }
