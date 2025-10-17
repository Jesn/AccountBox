using System.Security.Cryptography;
using System.Text;
using AccountBox.Core.Models.PasswordGenerator;

namespace AccountBox.Core.Services;

/// <summary>
/// 密码生成器服务
/// 使用密码学安全的随机数生成器生成强密码并计算密码强度
/// </summary>
public class PasswordGeneratorService
{
    // 字符集定义
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string NumberChars = "0123456789";
    private const string SymbolChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    // 易混淆字符
    private const string AmbiguousChars = "0O1lI";

    /// <summary>
    /// 生成密码
    /// </summary>
    public GeneratePasswordResponse Generate(GeneratePasswordRequest request)
    {
        // 验证请求
        ValidateRequest(request);

        // 根据是否启用字符分布生成密码
        string password;
        if (request.UseCharacterDistribution)
        {
            password = GeneratePasswordWithDistribution(request);
        }
        else
        {
            // 构建字符集
            var charset = BuildCharset(request);

            if (charset.Length == 0)
            {
                throw new ArgumentException("至少需要选择一种字符类型");
            }

            // 生成密码（原有逻辑）
            password = GenerateSecurePassword(charset, request.Length);
        }

        // 计算强度
        var strength = CalculatePasswordStrength(password);

        return new GeneratePasswordResponse
        {
            Password = password,
            Strength = strength
        };
    }

    /// <summary>
    /// 计算密码强度
    /// </summary>
    public PasswordStrengthResponse CalculateStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return new PasswordStrengthResponse
            {
                Strength = new PasswordStrength
                {
                    Score = 0,
                    Level = "Weak",
                    Length = 0,
                    Entropy = 0
                }
            };
        }

        var strength = CalculatePasswordStrength(password);

        return new PasswordStrengthResponse
        {
            Strength = strength
        };
    }

    /// <summary>
    /// 内部计算密码强度方法
    /// </summary>
    private PasswordStrength CalculatePasswordStrength(string password)
    {
        var hasUppercase = password.Any(char.IsUpper);
        var hasLowercase = password.Any(char.IsLower);
        var hasNumbers = password.Any(char.IsDigit);
        var hasSymbols = password.Any(c => !char.IsLetterOrDigit(c));

        // 计算字符集大小
        int charsetSize = 0;
        if (hasUppercase) charsetSize += 26;
        if (hasLowercase) charsetSize += 26;
        if (hasNumbers) charsetSize += 10;
        if (hasSymbols) charsetSize += 32; // 估算符号数量

        // 计算熵：log2(charsetSize^length)
        var entropy = password.Length * Math.Log2(charsetSize > 0 ? charsetSize : 1);

        // 计算分数 (0-100)
        int score = 0;

        // 长度分数 (最多 40 分)
        score += Math.Min(password.Length * 2, 40);

        // 字符多样性分数 (最多 40 分)
        if (hasUppercase) score += 10;
        if (hasLowercase) score += 10;
        if (hasNumbers) score += 10;
        if (hasSymbols) score += 10;

        // 熵分数 (最多 20 分)
        score += Math.Min((int)(entropy / 5), 20);

        // 限制在 0-100
        score = Math.Min(Math.Max(score, 0), 100);

        // 确定等级
        string level;
        if (score < 40)
            level = "Weak";
        else if (score < 60)
            level = "Medium";
        else if (score < 80)
            level = "Strong";
        else
            level = "VeryStrong";

        return new PasswordStrength
        {
            Score = score,
            Level = level,
            Length = password.Length,
            HasUppercase = hasUppercase,
            HasLowercase = hasLowercase,
            HasNumbers = hasNumbers,
            HasSymbols = hasSymbols,
            Entropy = Math.Round(entropy, 2)
        };
    }

    /// <summary>
    /// 使用密码学安全的随机数生成器生成密码
    /// </summary>
    private string GenerateSecurePassword(string charset, int length)
    {
        var password = new char[length];
        var charsetLength = charset.Length;

        // 使用 RandomNumberGenerator 生成密码学安全的随机数
        using (var rng = RandomNumberGenerator.Create())
        {
            var randomBytes = new byte[length * 4]; // 4 bytes per character for better distribution
            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                // 将 4 个字节转换为一个 uint，然后取模得到字符索引
                var randomValue = BitConverter.ToUInt32(randomBytes, i * 4);
                var index = randomValue % (uint)charsetLength;
                password[i] = charset[(int)index];
            }
        }

        return new string(password);
    }

    /// <summary>
    /// 按字符类型比例生成密码
    /// </summary>
    private string GeneratePasswordWithDistribution(GeneratePasswordRequest request)
    {
        // 计算各类型字符数量
        var counts = CalculateCharacterCounts(request);

        // 构建各类型字符集
        var charsets = new List<(string charset, int count)>();

        if (request.IncludeUppercase && counts.uppercase > 0)
        {
            var charset = request.ExcludeAmbiguous
                ? RemoveAmbiguousChars(UppercaseChars)
                : UppercaseChars;
            charsets.Add((charset, counts.uppercase));
        }

        if (request.IncludeLowercase && counts.lowercase > 0)
        {
            var charset = request.ExcludeAmbiguous
                ? RemoveAmbiguousChars(LowercaseChars)
                : LowercaseChars;
            charsets.Add((charset, counts.lowercase));
        }

        if (request.IncludeNumbers && counts.numbers > 0)
        {
            var charset = request.ExcludeAmbiguous
                ? RemoveAmbiguousChars(NumberChars)
                : NumberChars;
            charsets.Add((charset, counts.numbers));
        }

        if (request.IncludeSymbols && counts.symbols > 0)
        {
            charsets.Add((SymbolChars, counts.symbols));
        }

        if (charsets.Count == 0)
        {
            throw new ArgumentException("至少需要选择一种字符类型");
        }

        // 生成各类型字符
        var passwordChars = new List<char>();
        using (var rng = RandomNumberGenerator.Create())
        {
            foreach (var (charset, count) in charsets)
            {
                for (int i = 0; i < count; i++)
                {
                    var randomBytes = new byte[4];
                    rng.GetBytes(randomBytes);
                    var randomValue = BitConverter.ToUInt32(randomBytes, 0);
                    var index = randomValue % (uint)charset.Length;
                    passwordChars.Add(charset[(int)index]);
                }
            }

            // 打乱字符顺序
            for (int i = passwordChars.Count - 1; i > 0; i--)
            {
                var randomBytes = new byte[4];
                rng.GetBytes(randomBytes);
                var randomValue = BitConverter.ToUInt32(randomBytes, 0);
                var j = (int)(randomValue % (uint)(i + 1));
                (passwordChars[i], passwordChars[j]) = (passwordChars[j], passwordChars[i]);
            }
        }

        return new string(passwordChars.ToArray());
    }

    /// <summary>
    /// 计算各类型字符数量
    /// </summary>
    private (int uppercase, int lowercase, int numbers, int symbols) CalculateCharacterCounts(GeneratePasswordRequest request)
    {
        var totalPercentage = 0;
        var enabledTypes = 0;

        if (request.IncludeUppercase)
        {
            totalPercentage += request.UppercasePercentage;
            enabledTypes++;
        }
        if (request.IncludeLowercase)
        {
            totalPercentage += request.LowercasePercentage;
            enabledTypes++;
        }
        if (request.IncludeNumbers)
        {
            totalPercentage += request.NumbersPercentage;
            enabledTypes++;
        }
        if (request.IncludeSymbols)
        {
            totalPercentage += request.SymbolsPercentage;
            enabledTypes++;
        }

        if (enabledTypes == 0)
        {
            throw new ArgumentException("至少需要选择一种字符类型");
        }

        // 归一化百分比
        var uppercasePercent = request.IncludeUppercase ? (double)request.UppercasePercentage / totalPercentage : 0;
        var lowercasePercent = request.IncludeLowercase ? (double)request.LowercasePercentage / totalPercentage : 0;
        var numbersPercent = request.IncludeNumbers ? (double)request.NumbersPercentage / totalPercentage : 0;
        var symbolsPercent = request.IncludeSymbols ? (double)request.SymbolsPercentage / totalPercentage : 0;

        // 计算各类型字符数量
        var uppercaseCount = (int)Math.Floor(request.Length * uppercasePercent);
        var lowercaseCount = (int)Math.Floor(request.Length * lowercasePercent);
        var numbersCount = (int)Math.Floor(request.Length * numbersPercent);
        var symbolsCount = (int)Math.Floor(request.Length * symbolsPercent);

        // 处理舍入误差，将剩余字符分配给启用的类型
        var remaining = request.Length - (uppercaseCount + lowercaseCount + numbersCount + symbolsCount);
        if (remaining > 0)
        {
            if (request.IncludeLowercase) lowercaseCount += remaining;
            else if (request.IncludeUppercase) uppercaseCount += remaining;
            else if (request.IncludeNumbers) numbersCount += remaining;
            else if (request.IncludeSymbols) symbolsCount += remaining;
        }

        // 确保每种启用的类型至少有1个字符
        if (request.IncludeUppercase && uppercaseCount == 0) uppercaseCount = 1;
        if (request.IncludeLowercase && lowercaseCount == 0) lowercaseCount = 1;
        if (request.IncludeNumbers && numbersCount == 0) numbersCount = 1;
        if (request.IncludeSymbols && symbolsCount == 0) symbolsCount = 1;

        // 如果总数超过目标长度，按比例减少
        var total = uppercaseCount + lowercaseCount + numbersCount + symbolsCount;
        if (total > request.Length)
        {
            var excess = total - request.Length;
            // 从占比最大的类型中减少
            if (lowercaseCount >= uppercaseCount && lowercaseCount >= numbersCount && lowercaseCount >= symbolsCount)
                lowercaseCount -= excess;
            else if (uppercaseCount >= numbersCount && uppercaseCount >= symbolsCount)
                uppercaseCount -= excess;
            else if (numbersCount >= symbolsCount)
                numbersCount -= excess;
            else
                symbolsCount -= excess;
        }

        return (uppercaseCount, lowercaseCount, numbersCount, symbolsCount);
    }

    /// <summary>
    /// 移除易混淆字符
    /// </summary>
    private string RemoveAmbiguousChars(string input)
    {
        var result = input;
        foreach (var ambiguousChar in AmbiguousChars)
        {
            result = result.Replace(ambiguousChar.ToString(), string.Empty);
        }
        return result;
    }

    /// <summary>
    /// 构建字符集
    /// </summary>
    private string BuildCharset(GeneratePasswordRequest request)
    {
        var charset = new StringBuilder();

        if (request.IncludeUppercase)
            charset.Append(UppercaseChars);

        if (request.IncludeLowercase)
            charset.Append(LowercaseChars);

        if (request.IncludeNumbers)
            charset.Append(NumberChars);

        if (request.IncludeSymbols)
            charset.Append(SymbolChars);

        // 排除易混淆字符
        if (request.ExcludeAmbiguous)
        {
            var result = charset.ToString();
            foreach (var ambiguousChar in AmbiguousChars)
            {
                result = result.Replace(ambiguousChar.ToString(), string.Empty);
            }
            return result;
        }

        return charset.ToString();
    }

    /// <summary>
    /// 验证请求参数
    /// </summary>
    private void ValidateRequest(GeneratePasswordRequest request)
    {
        if (request.Length < 8)
        {
            throw new ArgumentException("密码长度不能少于 8 个字符", nameof(request.Length));
        }

        if (request.Length > 128)
        {
            throw new ArgumentException("密码长度不能超过 128 个字符", nameof(request.Length));
        }

        if (!request.IncludeUppercase && !request.IncludeLowercase &&
            !request.IncludeNumbers && !request.IncludeSymbols)
        {
            throw new ArgumentException("至少需要选择一种字符类型");
        }
    }
}
