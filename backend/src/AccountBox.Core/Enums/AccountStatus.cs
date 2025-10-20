namespace AccountBox.Core.Enums;

/// <summary>
/// 账号状态枚举
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// 活跃状态（可正常使用）
    /// </summary>
    Active = 0,

    /// <summary>
    /// 已禁用状态（暂停使用，但未删除）
    /// </summary>
    Disabled = 1
}
