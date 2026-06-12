using System.Text;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Auth;
using AccountBox.Core.Models.Configuration;
using AccountBox.Core.Services;
using AccountBox.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AccountBox.Api.Extensions;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAccountBoxAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var storageOptions = ResolveSecurityStorageOptions(configuration);
        services.AddSingleton(storageOptions);

        services.AddSingleton<ISecretsManager, SecretsManager>();
        services.AddScoped<IJwtKeyRotationService, JwtKeyRotationService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddMemoryCache();

        services.Configure<JwtSettings>(options =>
        {
            var jwtConfig = configuration.GetSection("JwtSettings");
            options.SecretKey = string.Empty;
            options.Issuer = jwtConfig["Issuer"] ?? "AccountBox";
            options.Audience = jwtConfig["Audience"] ?? "AccountBox-Web";
            options.ExpirationHours = int.Parse(jwtConfig["ExpirationHours"] ?? "24");
            options.ValidateIssuer = bool.Parse(jwtConfig["ValidateIssuer"] ?? "true");
            options.ValidateAudience = bool.Parse(jwtConfig["ValidateAudience"] ?? "true");
            options.ValidateLifetime = bool.Parse(jwtConfig["ValidateLifetime"] ?? "true");
            options.ValidateIssuerSigningKey = bool.Parse(jwtConfig["ValidateIssuerSigningKey"] ?? "true");
        });

        var jwtSettings = ResolveJwtSettings(configuration);

        services.AddAuthentication(options =>
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
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey
            };
        });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IServiceScopeFactory>((options, scopeFactory) =>
            {
                options.TokenValidationParameters.IssuerSigningKeyResolver = (_, _, _, _) =>
                {
                    using var scope = scopeFactory.CreateScope();
                    var keyRotationService = scope.ServiceProvider.GetRequiredService<IJwtKeyRotationService>();
                    return keyRotationService.GetValidationKeys()
                        .Select(keyVersion => new SymmetricSecurityKey(GetSigningKeyBytes(keyVersion.Key))
                        {
                            KeyId = keyVersion.Id
                        });
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static JwtSettings ResolveJwtSettings(IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection("JwtSettings");
        return new JwtSettings
        {
            SecretKey = string.Empty,
            Issuer = jwtConfig["Issuer"] ?? "AccountBox",
            Audience = jwtConfig["Audience"] ?? "AccountBox-Web",
            ExpirationHours = int.Parse(jwtConfig["ExpirationHours"] ?? "24"),
            ValidateIssuer = bool.Parse(jwtConfig["ValidateIssuer"] ?? "true"),
            ValidateAudience = bool.Parse(jwtConfig["ValidateAudience"] ?? "true"),
            ValidateLifetime = bool.Parse(jwtConfig["ValidateLifetime"] ?? "true"),
            ValidateIssuerSigningKey = bool.Parse(jwtConfig["ValidateIssuerSigningKey"] ?? "true")
        };
    }

    private static SecurityStorageOptions ResolveSecurityStorageOptions(IConfiguration configuration)
    {
        var dataPath = Environment.GetEnvironmentVariable(AccountBox.Core.Configuration.AccountBoxEnvironment.DataPath)
                       ?? configuration["Storage:DataPath"]
                       ?? Path.Combine(Directory.GetCurrentDirectory(), "data");

        return new SecurityStorageOptions
        {
            DataPath = dataPath
        };
    }

    private static byte[] GetSigningKeyBytes(string key)
    {
        try
        {
            return Convert.FromBase64String(key);
        }
        catch
        {
            return Encoding.UTF8.GetBytes(key);
        }
    }
}