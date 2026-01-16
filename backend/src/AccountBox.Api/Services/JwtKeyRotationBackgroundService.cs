using AccountBox.Core.Interfaces;

namespace AccountBox.Api.Services;

/// <summary>
/// JWT密钥自动轮换后台服务
/// </summary>
public class JwtKeyRotationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JwtKeyRotationBackgroundService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly int _rotationPolicyDays;
    private readonly int _transitionPeriodDays;

    public JwtKeyRotationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<JwtKeyRotationBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // 从配置读取轮换策略
        _checkInterval = TimeSpan.FromHours(
            configuration.GetValue("JwtKeyRotation:CheckIntervalHours", 24));
        _rotationPolicyDays = configuration.GetValue("JwtKeyRotation:RotationPolicyDays", 30);
        _transitionPeriodDays = configuration.GetValue("JwtKeyRotation:TransitionPeriodDays", 7);

        _logger.LogInformation("JWT密钥自动轮换服务已配置：");
        _logger.LogInformation("  - 检查间隔: {Interval}", _checkInterval);
        _logger.LogInformation("  - 轮换周期: {Days} 天", _rotationPolicyDays);
        _logger.LogInformation("  - 过渡期: {Days} 天", _transitionPeriodDays);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JWT密钥自动轮换服务已启动");

        // 启动时延迟1分钟执行第一次检查
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformRotationCheckAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "密钥轮换检查失败");
            }

            // 等待下次检查
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("JWT密钥自动轮换服务已停止");
    }

    private async Task PerformRotationCheckAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var keyRotationService = scope.ServiceProvider.GetRequiredService<IJwtKeyRotationService>();

        _logger.LogDebug("执行密钥轮换检查...");

        // 检查是否需要轮换
        if (keyRotationService.ShouldRotate(_rotationPolicyDays))
        {
            _logger.LogWarning("检测到密钥需要轮换（距上次轮换已超过 {Days} 天）", _rotationPolicyDays);

            try
            {
                var newKey = await keyRotationService.RotateKeyAsync(_transitionPeriodDays);
                _logger.LogWarning("自动密钥轮换成功：新密钥 {KeyId} 已激活", newKey.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自动密钥轮换失败");
            }
        }
        else
        {
            var keyStore = keyRotationService.GetKeyStore();
            var nextRotation = keyStore.LastRotationAt?.AddDays(_rotationPolicyDays);
            var daysUntilRotation = nextRotation.HasValue
                ? (nextRotation.Value - DateTime.UtcNow).Days
                : 0;

            _logger.LogDebug("密钥无需轮换（距下次轮换还有 {Days} 天）", daysUntilRotation);
        }

        // 清理过期密钥
        try
        {
            await keyRotationService.CleanupExpiredKeysAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期密钥失败");
        }
    }
}
