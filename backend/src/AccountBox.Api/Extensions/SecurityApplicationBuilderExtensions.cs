using AccountBox.Core.Interfaces;

namespace AccountBox.Api.Extensions;

public static class SecurityApplicationBuilderExtensions
{
    public static void UseAccountBoxSecurityInitialization(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var secretsManager = scope.ServiceProvider.GetRequiredService<ISecretsManager>();
        secretsManager.GetOrGenerateMasterPasswordHash();
    }
}