using System.ComponentModel.DataAnnotations;

namespace AccountBox.Core.Models.Auth;

/// <summary>
/// 登录请求DTO
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 主密码
    /// </summary>
    [Required(ErrorMessage = "主密码不能为空")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "主密码长度必须在1-1000字符之间")]
    public required string MasterPassword { get; set; }
}
