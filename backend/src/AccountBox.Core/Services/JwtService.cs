using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AccountBox.Core.Services;

/// <summary>
/// JWT Token服务实现（支持多密钥版本验证）
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IJwtKeyRotationService _keyRotationService;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ILogger<JwtService> _logger;

    public JwtService(
        IOptions<JwtSettings> jwtSettings,
        IJwtKeyRotationService keyRotationService,
        ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _keyRotationService = keyRotationService;
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>
    /// 生成JWT Token（使用当前主密钥）
    /// </summary>
    public (string Token, DateTime ExpiresAt) GenerateToken(string subject)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(_jwtSettings.ExpirationHours);

        // 获取当前主密钥
        var currentKey = _keyRotationService.GetCurrentKey();
        var signingKey = GetSymmetricKey(currentKey.Key);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("kid", currentKey.Id) // Key ID - 用于多密钥验证
        };

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = _tokenHandler.WriteToken(token);

        _logger.LogDebug("生成JWT Token，使用密钥版本: {KeyId}", currentKey.Id);

        return (tokenString, expiresAt);
    }

    /// <summary>
    /// 验证JWT Token（支持多密钥版本）
    /// </summary>
    public bool ValidateToken(string token)
    {
        try
        {
            // 尝试使用所有有效密钥进行验证
            var validationKeys = _keyRotationService.GetValidationKeys();

            foreach (var keyVersion in validationKeys)
            {
                try
                {
                    var signingKey = GetSymmetricKey(keyVersion.Key);
                    var validationParameters = GetValidationParameters(signingKey);
                    _tokenHandler.ValidateToken(token, validationParameters, out _);

                    _logger.LogDebug("Token验证成功，使用密钥版本: {KeyId}", keyVersion.Id);
                    return true;
                }
                catch (SecurityTokenException)
                {
                    // 当前密钥验证失败，尝试下一个
                    continue;
                }
            }

            _logger.LogWarning("Token验证失败：所有密钥版本都无法验证此Token");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token验证过程中发生异常");
            return false;
        }
    }

    /// <summary>
    /// 从Token中提取Claims（支持多密钥版本）
    /// </summary>
    public ClaimsPrincipal? GetClaimsFromToken(string token)
    {
        try
        {
            // 尝试使用所有有效密钥进行验证
            var validationKeys = _keyRotationService.GetValidationKeys();

            foreach (var keyVersion in validationKeys)
            {
                try
                {
                    var signingKey = GetSymmetricKey(keyVersion.Key);
                    var validationParameters = GetValidationParameters(signingKey);
                    var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);

                    return principal;
                }
                catch (SecurityTokenException)
                {
                    // 当前密钥验证失败，尝试下一个
                    continue;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取Token验证参数
    /// </summary>
    private TokenValidationParameters GetValidationParameters(SymmetricSecurityKey signingKey)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = _jwtSettings.ValidateIssuer,
            ValidIssuer = _jwtSettings.Issuer,

            ValidateAudience = _jwtSettings.ValidateAudience,
            ValidAudience = _jwtSettings.Audience,

            ValidateLifetime = _jwtSettings.ValidateLifetime,
            ClockSkew = TimeSpan.Zero, // 不允许时钟偏移

            ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
            IssuerSigningKey = signingKey
        };
    }

    /// <summary>
    /// 获取对称密钥
    /// </summary>
    private SymmetricSecurityKey GetSymmetricKey(string key)
    {
        // 尝试将密钥作为 Base64 解码，如果失败则使用 UTF-8 编码
        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(key);
        }
        catch
        {
            // 如果不是 Base64，则使用 UTF-8 编码
            keyBytes = Encoding.UTF8.GetBytes(key);
        }

        // 确保密钥长度至少为 32 字节（256 位）
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                $"JWT 密钥长度不足：需要至少 32 字节（256 位），当前为 {keyBytes.Length} 字节。");
        }

        return new SymmetricSecurityKey(keyBytes);
    }
}