namespace AccountBox.Core.Models.Configuration;

/// <summary>
/// 数据库运行时配置。
/// </summary>
public class DatabaseOptions
{
    public string Provider { get; init; } = "sqlite";

    public string? ConnectionString { get; init; }

    public string? SqliteDatabasePath { get; init; }

    public string NormalizedProvider => Provider.Trim().ToLowerInvariant();

    public bool IsPostgreSql => NormalizedProvider is "postgresql" or "postgres";

    public bool IsMySql => NormalizedProvider == "mysql";
}