using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccountBox.Data.DbContext;

/// <summary>
/// 设计时 DbContext 工厂，用于 EF Core 迁移工具
/// </summary>
public class AccountBoxDbContextFactory : IDesignTimeDbContextFactory<AccountBoxDbContext>
{
    public AccountBoxDbContext CreateDbContext(string[] args)
    {
        // 从环境变量或命令行参数获取数据库提供程序
        var provider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "sqlite";

        // 从命令行参数解析提供程序（优先级更高）
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--provider" && i + 1 < args.Length)
            {
                provider = args[i + 1].ToLower();
                break;
            }
        }

        var optionsBuilder = new DbContextOptionsBuilder<AccountBoxDbContext>();

        switch (provider.ToLower())
        {
            case "postgresql":
            case "postgres":
                var postgresConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                    ?? "Host=localhost;Port=5432;Database=accountbox;Username=accountbox;Password=accountbox123";
                optionsBuilder.UseNpgsql(postgresConnectionString,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory_PostgreSQL", "public")
                              .MigrationsAssembly("AccountBox.Data.Migrations.PostgreSQL"));
                break;

            case "mysql":
                var mysqlConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                    ?? "Server=localhost;Port=3306;Database=accountbox;User=accountbox;Password=accountbox123";
                optionsBuilder.UseMySql(mysqlConnectionString, ServerVersion.AutoDetect(mysqlConnectionString),
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory_MySQL")
                              .MigrationsAssembly("AccountBox.Data.Migrations.MySQL"));
                break;

            case "sqlite":
            default:
                var sqliteConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                    ?? "Data Source=accountbox.db";
                optionsBuilder.UseSqlite(sqliteConnectionString,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory_Sqlite")
                              .MigrationsAssembly("AccountBox.Data.Migrations.Sqlite"));
                break;
        }

        return new AccountBoxDbContext(optionsBuilder.Options);
    }
}
