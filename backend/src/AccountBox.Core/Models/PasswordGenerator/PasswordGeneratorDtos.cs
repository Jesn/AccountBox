namespace AccountBox.Core.Models.PasswordGenerator;

/// <summary>
/// 生成密码请求
/// </summary>
public class GeneratePasswordRequest
{
    /// <summary>
    /// 密码长度 (最小8, 最大128)
    /// </summary>
    public int Length { get; set; } = 16;

    /// <summary>
    /// 包含大写字母 (A-Z)
    /// </summary>
    public bool IncludeUppercase { get; set; } = true;

    /// <summary>
    /// 包含小写字母 (a-z)
    /// </summary>
    public bool IncludeLowercase { get; set; } = true;

    /// <summary>
    /// 包含数字 (0-9)
    /// </summary>
    public bool IncludeNumbers { get; set; } = true;

    /// <summary>
    /// 包含符号 (!@#$%^&*()_+-=[]{}|;:,.<>?)
    /// </summary>
    public bool IncludeSymbols { get; set; } = true;

    /// <summary>
    /// 排除易混淆字符 (0O, 1lI, etc.)
    /// </summary>
    public bool ExcludeAmbiguous { get; set; } = false;
}

/// <summary>
/// 生成密码响应
/// </summary>
public class GeneratePasswordResponse
{
    /// <summary>
    /// 生成的密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 密码强度信息
    /// </summary>
    public PasswordStrength Strength { get; set; } = null!;
}

/// <summary>
/// 计算密码强度请求
/// </summary>
public class CalculateStrengthRequest
{
    /// <summary>
    /// 要计算强度的密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 密码强度响应
/// </summary>
public class PasswordStrengthResponse
{
    /// <summary>
    /// 密码强度信息
    /// </summary>
    public PasswordStrength Strength { get; set; } = null!;
}

/// <summary>
/// 密码强度信息
/// </summary>
public class PasswordStrength
{
    /// <summary>
    /// 强度分数 (0-100)
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// 强度等级 (Weak, Medium, Strong, VeryStrong)
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// 密码长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// 是否包含大写字母
    /// </summary>
    public bool HasUppercase { get; set; }

    /// <summary>
    /// 是否包含小写字母
    /// </summary>
    public bool HasLowercase { get; set; }

    /// <summary>
    /// 是否包含数字
    /// </summary>
    public bool HasNumbers { get; set; }

    /// <summary>
    /// 是否包含符号
    /// </summary>
    public bool HasSymbols { get; set; }

    /// <summary>
    /// 估算的熵（比特）
    /// </summary>
    public double Entropy { get; set; }
}
