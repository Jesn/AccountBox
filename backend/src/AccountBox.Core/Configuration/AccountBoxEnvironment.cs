namespace AccountBox.Core.Configuration;

/// <summary>
/// AccountBox 环境变量名称。
/// </summary>
public static class AccountBoxEnvironment
{
    public const string DatabaseProvider = "DB_PROVIDER";
    public const string ConnectionString = "CONNECTION_STRING";
    public const string DatabasePath = "DATABASE_PATH";
    public const string DataPath = "DATA_PATH";
    public const string JwtSecretKey = "JWT_SECRET_KEY";
    public const string MasterPassword = "MASTER_PASSWORD";
}