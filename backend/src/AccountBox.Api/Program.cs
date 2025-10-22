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
                b =>
                {
                    b.MigrationsHistoryTable("__EFMigrationsHistory_PostgreSQL", "public")
                     .MigrationsAssembly("AccountBox.Data.Migrations.PostgreSQL")
                     .CommandTimeout(120); // 迁移命令超时设置为 120 秒
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
        }
        else
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsHistoryTable("__EFMigrationsHistory_PostgreSQL", "public")
                     .MigrationsAssembly("AccountBox.Data.Migrations.PostgreSQL")
                     .CommandTimeout(120);
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                });
        }
    }
    else if (dbProvider == "mysql")
    {
        // 使用 MySQL
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                b =>
                {
                    b.MigrationsHistoryTable("__EFMigrationsHistory_MySQL")
                     .MigrationsAssembly("AccountBox.Data.Migrations.MySQL")
                     .CommandTimeout(120); // 迁移命令超时设置为 120 秒
                    b.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
        }
        else
        {
            var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (defaultConnectionString != null)
            {
                options.UseMySql(defaultConnectionString, ServerVersion.AutoDetect(defaultConnectionString),
                    b =>
                    {
                        b.MigrationsHistoryTable("__EFMigrationsHistory_MySQL")
                         .MigrationsAssembly("AccountBox.Data.Migrations.MySQL")
                         .CommandTimeout(120);
                        b.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
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
builder.Services.AddScoped<IWebsiteRepository, WebsiteRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();

// 配置依赖注入 - 业务服务层
builder.Services.AddScoped<IWebsiteService, WebsiteService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRecycleBinService, RecycleBinService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<AccountBox.Core.Services.PasswordGeneratorService>();
builder.Services.AddScoped<AccountBox.Core.Services.IApiKeyService, ApiKeyService>();

builder.Services.AddScoped<IApiKeysManagementService, ApiKeysManagementService>();
builder.Services.AddScoped<IRandomAccountService, RandomAccountService>();

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

// 统一密钥处理方式：如果是 Base64 则先解码，否则按 UTF-8 文本处理
// 这样与 JwtService 中的密钥处理逻辑保持一致，避免签发/验证使用不同字节导致 401
byte[] signingKeyBytes;
try
{
    signingKeyBytes = Convert.FromBase64String(jwtSettings.SecretKey);
}
catch
{
    signingKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
}
var signingKey = new SymmetricSecurityKey(signingKeyBytes);

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
        IssuerSigningKey = signingKey
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountBoxDbContext>();
    var dbLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER")?.ToLower() ?? "sqlite";

    try
    {
        dbLogger.LogInformation("========================================");
        dbLogger.LogInformation("开始数据库迁移检查");
        dbLogger.LogInformation($"数据库类型: {dbProvider.ToUpper()}");
        dbLogger.LogInformation("========================================");

        // 等待数据库就绪（针对 Docker 容器启动时序问题）
        var maxRetries = 30; // 最多等待 30 秒
        var retryCount = 0;
        var connected = false;

        while (retryCount < maxRetries && !connected)
        {
            try
            {
                connected = db.Database.CanConnect();
                if (!connected)
                {
                    retryCount++;
                    if (retryCount == 1)
                    {
                        dbLogger.LogInformation("等待数据库就绪...");
                    }
                    Thread.Sleep(1000); // 等待 1 秒后重试
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    dbLogger.LogError(ex, "数据库连接失败，已达到最大重试次数");
                    throw;
                }
                Thread.Sleep(1000);
            }
        }

        if (!connected)
        {
            var errorMessage = dbProvider switch
            {
                "postgresql" or "postgres" => "无法连接到 PostgreSQL 数据库。请确保：\n" +
                    "1. PostgreSQL 服务已启动\n" +
                    "2. 数据库已创建（通过 Docker Compose 或手动创建）\n" +
                    "3. 连接字符串配置正确（CONNECTION_STRING 环境变量）",
                "mysql" => "无法连接到 MySQL 数据库。请确保：\n" +
                    "1. MySQL 服务已启动\n" +
                    "2. 数据库已创建（通过 Docker Compose 或手动创建）\n" +
                    "3. 连接字符串配置正确（CONNECTION_STRING 环境变量）",
                _ => "无法连接到 SQLite 数据库。请检查 DATABASE_PATH 环境变量配置。"
            };
            dbLogger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        dbLogger.LogInformation("✓ 数据库连接成功");

        // 获取待应用的迁移
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        var appliedMigrations = db.Database.GetAppliedMigrations().ToList();

        dbLogger.LogInformation($"已应用的迁移数量: {appliedMigrations.Count}");
        dbLogger.LogInformation($"待应用的迁移数量: {pendingMigrations.Count}");

        if (pendingMigrations.Any())
        {
            dbLogger.LogInformation("----------------------------------------");
            dbLogger.LogInformation("发现待应用的迁移，开始应用...");
            foreach (var migration in pendingMigrations)
            {
                dbLogger.LogInformation($"  → {migration}");
            }
            dbLogger.LogInformation("----------------------------------------");

            var startTime = DateTime.UtcNow;

            // 应用所有待处理的迁移
            db.Database.Migrate();

            var duration = DateTime.UtcNow - startTime;
            dbLogger.LogInformation($"✓ 数据库迁移成功完成（耗时: {duration.TotalSeconds:F2} 秒）");
        }
        else
        {
            dbLogger.LogInformation("✓ 数据库已是最新版本，无需迁移");
        }

        dbLogger.LogInformation("========================================");
        dbLogger.LogInformation("数据库迁移检查完成");
        dbLogger.LogInformation("========================================");
    }
    catch (Exception ex)
    {
        dbLogger.LogError("========================================");
        dbLogger.LogError(ex, "❌ 数据库迁移失败");
        dbLogger.LogError("========================================");
        dbLogger.LogError("故障排查建议：");
        dbLogger.LogError("1. 检查数据库服务是否正常运行");
        dbLogger.LogError("2. 检查连接字符串配置是否正确");
        dbLogger.LogError("3. 检查数据库用户权限是否足够");
        dbLogger.LogError("4. 查看上方详细错误信息");
        dbLogger.LogError("========================================");
        throw;
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
