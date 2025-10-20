namespace AccountBox.Core.Exceptions;

/// <summary>
/// 当网站下还有活跃账号时尝试删除网站抛出的异常
/// </summary>
public class ActiveAccountsExistException : InvalidOperationException
{
    public int WebsiteId { get; }
    public int ActiveAccountCount { get; }

    public ActiveAccountsExistException(int websiteId, int activeAccountCount)
        : base($"Cannot delete website {websiteId}: {activeAccountCount} active account(s) exist. Please delete or move them to recycle bin first.")
    {
        WebsiteId = websiteId;
        ActiveAccountCount = activeAccountCount;
    }
}

/// <summary>
/// 当网站下有回收站账号需要用户确认删除时抛出的异常
/// </summary>
public class ConfirmationRequiredException : InvalidOperationException
{
    public int WebsiteId { get; }
    public int DeletedAccountCount { get; }

    public ConfirmationRequiredException(int websiteId, int deletedAccountCount)
        : base($"Cannot delete website {websiteId}: {deletedAccountCount} deleted account(s) in recycle bin. Confirmation required.")
    {
        WebsiteId = websiteId;
        DeletedAccountCount = deletedAccountCount;
    }
}
