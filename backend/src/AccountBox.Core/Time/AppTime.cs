using AccountBox.Core.Configuration;

namespace AccountBox.Core.Time;

/// <summary>
/// 应用统一时钟。
/// 业务时间戳统一使用配置时区下的当前墙钟时间（不是 UTC）。
/// 配置来源：环境变量 TZ 或 APP_TIMEZONE（IANA，如 Asia/Shanghai）。
/// </summary>
/// <remarks>
/// JWT 等协议字段仍应使用 <see cref="DateTime.UtcNow"/>，不要用本类。
/// </remarks>
public static class AppTime
{
    private static TimeZoneInfo _timeZone = TimeZoneInfo.Local;

    /// <summary>
    /// 当前生效的应用时区。
    /// </summary>
    public static TimeZoneInfo TimeZone => _timeZone;

    /// <summary>
    /// 时区标识（便于日志与健康检查）。
    /// </summary>
    public static string TimeZoneId => _timeZone.Id;

    /// <summary>
    /// 业务当前时间：配置时区下的墙钟时间，Kind 固定为 Unspecified，
    /// 避免 JSON 被序列化成带 Z 的 UTC，导致前端二次转换。
    /// </summary>
    public static DateTime Now
    {
        get
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
            return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        }
    }

    /// <summary>
    /// 从环境变量初始化应用时区。应在应用启动最早阶段调用一次。
    /// </summary>
    public static void ConfigureFromEnvironment()
    {
        var id = Environment.GetEnvironmentVariable(AccountBoxEnvironment.TimeZone)
                 ?? Environment.GetEnvironmentVariable(AccountBoxEnvironment.AppTimeZone);

        if (string.IsNullOrWhiteSpace(id))
        {
            _timeZone = TimeZoneInfo.Local;
            return;
        }

        _timeZone = ResolveTimeZone(id.Trim());
    }

    private static TimeZoneInfo ResolveTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // Windows 常见别名兼容
            var windowsId = MapIanaToWindows(id);
            if (windowsId is not null)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                }
                catch (Exception)
                {
                    // fall through
                }
            }

            // 解析失败时回退本地时区，避免启动崩溃
            return TimeZoneInfo.Local;
        }
    }

    private static string? MapIanaToWindows(string ianaId) => ianaId switch
    {
        "Asia/Shanghai" or "Asia/Chongqing" or "Asia/Harbin" or "PRC" => "China Standard Time",
        "UTC" or "Etc/UTC" or "Etc/GMT" => "UTC",
        _ => null
    };
}
