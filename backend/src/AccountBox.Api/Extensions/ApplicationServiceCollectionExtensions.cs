using AccountBox.Api.Services;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Services;
using AccountBox.Data.Repositories;

namespace AccountBox.Api.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddAccountBoxApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IWebsiteRepository, WebsiteRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ISearchRepository, SearchRepository>();

        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IRecycleBinService, RecycleBinService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<PasswordGeneratorService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IApiKeysManagementService, ApiKeysManagementService>();
        services.AddScoped<IRandomAccountService, RandomAccountService>();
        services.AddScoped<ILoginAttemptService, LoginAttemptService>();

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddAccountBoxApiDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? new[] { "http://localhost:5093", "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddControllers();

        return services;
    }
}