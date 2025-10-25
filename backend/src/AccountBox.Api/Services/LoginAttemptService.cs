using AccountBox.Core.Interfaces;
using AccountBox.Data.DbContext;
using AccountBox.Data.Entities;

namespace AccountBox.Api.Services;

/// <summary>
/// 登录尝试记录服务实现
/// </summary>
public class LoginAttemptService : ILoginAttemptService
{
    private readonly AccountBoxDbContext _dbContext;
    private readonly ILogger<LoginAttemptService> _logger;

    public LoginAttemptService(AccountBoxDbContext dbContext, ILogger<LoginAttemptService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 记录登录尝试（成功或失败）
    /// </summary>
    public async Task RecordLoginAttemptAsync(string ipAddress, string? userAgent, bool isSuccessful, string? failureReason = null)
    {
        try
        {
            var loginAttempt = new LoginAttempt
            {
                IPAddress = ipAddress,
                AttemptTime = DateTime.UtcNow,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason,
                UserAgent = userAgent
            };

            _dbContext.LoginAttempts.Add(loginAttempt);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 记录失败不应该影响登录流程
            _logger.LogError(ex, "Failed to record login attempt");
        }
    }
}