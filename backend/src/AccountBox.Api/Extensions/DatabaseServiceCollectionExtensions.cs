using AccountBox.Core.Configuration;
using AccountBox.Core.Models.Configuration;
using AccountBox.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace AccountBox.Api.Extensions;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddAccountBoxDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = ResolveDatabaseOptions(configuration);
        services.AddSingleton(databaseOptions);

        services.AddDbContext<AccountBoxDbContext>(options =>
        {
            if (databaseOptions.IsPostgreSql)
            {
                options.UseNpgsql(databaseOptions.ConnectionString,
                    builder =>
                    {
                        builder.MigrationsHistoryTable("__EFMigrationsHistory_PostgreSQL", "public")
                               .MigrationsAssembly("AccountBox.Data.Migrations.PostgreSQL")
                               .CommandTimeout(120);
                        builder.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null);
                    });
                return;
            }

            if (databaseOptions.IsMySql)
            {
                if (string.IsNullOrWhiteSpace(databaseOptions.ConnectionString))
                {
                    throw new InvalidOperationException("MySQL 连接字符串不能为空。请配置 CONNECTION_STRING 或 ConnectionStrings:DefaultConnection。");
                }

                options.UseMySql(databaseOptions.ConnectionString, ServerVersion.AutoDetect(databaseOptions.ConnectionString),
                    builder =>
                    {
                        builder.MigrationsHistoryTable("__EFMigrationsHistory_MySQL")
                               .MigrationsAssembly("AccountBox.Data.Migrations.MySQL")
                               .CommandTimeout(120);
                        builder.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
                return;
            }

            var sqliteConnectionString = !string.IsNullOrWhiteSpace(databaseOptions.SqliteDatabasePath)
                ? $"Data Source={databaseOptions.SqliteDatabasePath}"
                : databaseOptions.ConnectionString;

            options.UseSqlite(sqliteConnectionString,
                builder => builder.MigrationsHistoryTable("__EFMigrationsHistory_Sqlite")
                                  .MigrationsAssembly("AccountBox.Data.Migrations.Sqlite"));
        });

        return services;
    }

    private static DatabaseOptions ResolveDatabaseOptions(IConfiguration configuration)
    {
        var provider = Environment.GetEnvironmentVariable(AccountBoxEnvironment.DatabaseProvider)
                       ?? configuration["Database:Provider"]
                       ?? "sqlite";

        var connectionString = Environment.GetEnvironmentVariable(AccountBoxEnvironment.ConnectionString)
                               ?? configuration.GetConnectionString("DefaultConnection");

        var sqliteDatabasePath = Environment.GetEnvironmentVariable(AccountBoxEnvironment.DatabasePath)
                                 ?? configuration["Database:SqliteDatabasePath"];

        return new DatabaseOptions
        {
            Provider = provider,
            ConnectionString = connectionString,
            SqliteDatabasePath = sqliteDatabasePath
        };
    }
}