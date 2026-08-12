using AccountBox.Api.Extensions;
using AccountBox.Api.Middleware;
using AccountBox.Core.Time;
using Microsoft.Extensions.FileProviders;

// PostgreSQL timestamptz 兼容：允许 Unspecified 墙钟时间写入
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 统一应用时区（读取 TZ / APP_TIMEZONE，默认系统本地时区）
AppTime.ConfigureFromEnvironment();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAccountBoxApiDefaults(builder.Configuration);
builder.Services.AddAccountBoxDatabase(builder.Configuration);
builder.Services.AddAccountBoxApplicationServices();
builder.Services.AddAccountBoxAuthentication(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

var webRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
PhysicalFileProvider? fileProvider = null;
if (Directory.Exists(webRoot))
{
    fileProvider = new PhysicalFileProvider(webRoot);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
}

app.UseAccountBoxDatabaseMigration();
app.UseAccountBoxSecurityInitialization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/external"),
    appBuilder => appBuilder.UseMiddleware<ApiKeyAuthMiddleware>()
);

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        timestamp = AppTime.Now,
        timeZone = AppTime.TimeZoneId
    }))
    .WithName("HealthCheck")
    .WithOpenApi();

if (fileProvider is not null)
{
    app.MapFallback(async context =>
    {
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

public partial class Program { }