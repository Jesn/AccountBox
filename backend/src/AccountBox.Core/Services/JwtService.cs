using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccountBox.Core.Interfaces;
using AccountBox.Core.Models.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AccountBox.Core.Services;

/// <summary>
/// JWT Token服务实现
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;

        // 尝试将密钥作为 Base64 解码，如果失败则使用 UTF-8 编码
        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(_jwtSettings.SecretKey);
        }
        catch
        {
            // 如果不是 Base64，则使用 UTF-8 编码
            keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        }

        // 确保密钥长度至少为 32 字节（256 位）
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                $"JWT 密钥长度不足：需要至少 32 字节（256 位），当前为 {keyBytes.Length} 字节。" +
                "请删除旧的密钥文件并重新生成，或设置环境变量 JWT_SECRET_KEY。");
        }

        _signingKey = new SymmetricSecurityKey(keyBytes);
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <summary>
    /// 生成JWT Token
    /// </summary>
    public (string Token, DateTime ExpiresAt) GenerateToken(string subject)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddHours(_jwtSettings.ExpirationHours);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

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

        return (tokenString, expiresAt);
    }

    /// <summary>
    /// 验证JWT Token
    /// </summary>
    public bool ValidateToken(string token)
    {
        try
        {
            var validationParameters = GetValidationParameters();
            _tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 从Token中提取Claims
    /// </summary>
    public ClaimsPrincipal? GetClaimsFromToken(string token)
    {
        try
        {
            var validationParameters = GetValidationParameters();
            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取Token验证参数
    /// </summary>
    private TokenValidationParameters GetValidationParameters()
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
            IssuerSigningKey = _signingKey
        };
    }
}