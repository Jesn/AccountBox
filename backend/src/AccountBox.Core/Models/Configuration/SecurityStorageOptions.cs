namespace AccountBox.Core.Models.Configuration;

/// <summary>
/// 本地安全数据存储配置。
/// </summary>
public class SecurityStorageOptions
{
    public string DataPath { get; init; } = Path.Combine(Directory.GetCurrentDirectory(), "data");

    public string SecretsDirectory => Path.Combine(DataPath, ".secrets");

    public string LegacyJwtKeyPath => Path.Combine(SecretsDirectory, "jwt.key");

    public string JwtKeyStorePath => Path.Combine(SecretsDirectory, "jwt_keys.json");

    public string MasterPasswordPath => Path.Combine(SecretsDirectory, "master.key");
}