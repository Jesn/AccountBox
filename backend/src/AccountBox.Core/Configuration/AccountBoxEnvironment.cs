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

    /// <summary>
    /// 标准时区环境变量（Docker/Linux 常用，如 Asia/Shanghai）。
    /// </summary>
    public const string TimeZone = "TZ";

    /// <summary>
    /// 应用时区别名（与 TZ 等价，便于显式配置）。
    /// </summary>
    public const string AppTimeZone = "APP_TIMEZONE";
}