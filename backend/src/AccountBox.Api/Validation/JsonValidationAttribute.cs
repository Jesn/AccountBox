using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace AccountBox.Api.Validation;

/// <summary>
/// JSON 字段验证属性
/// 验证 JSON 格式和大小限制
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class JsonValidationAttribute : ValidationAttribute
{
    /// <summary>
    /// 最大 JSON 大小（字节）
    /// </summary>
    public int MaxSizeBytes { get; set; } = 10 * 1024; // 默认 10KB

    /// <summary>
    /// 是否允许空值
    /// </summary>
    public bool AllowNull { get; set; } = true;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 如果值为 null
        if (value == null)
        {
            return AllowNull
                ? ValidationResult.Success
                : new ValidationResult("JSON 字段不能为空");
        }

        // 如果是字符串类型
        if (value is string jsonString)
        {
            // 检查是否为空字符串
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return AllowNull
                    ? ValidationResult.Success
                    : new ValidationResult("JSON 字符串不能为空");
            }

            // 验证 JSON 格式
            try
            {
                JsonDocument.Parse(jsonString);
            }
            catch (JsonException)
            {
                return new ValidationResult("无效的 JSON 格式");
            }

            // 验证大小
            var sizeInBytes = Encoding.UTF8.GetByteCount(jsonString);
            if (sizeInBytes > MaxSizeBytes)
            {
                return new ValidationResult($"JSON 大小超过限制 ({MaxSizeBytes} 字节)");
            }

            return ValidationResult.Success;
        }

        // 如果是 Dictionary 或其他对象类型
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            var sizeInBytes = Encoding.UTF8.GetByteCount(serialized);

            if (sizeInBytes > MaxSizeBytes)
            {
                return new ValidationResult($"JSON 大小超过限制 ({MaxSizeBytes} 字节)");
            }

            return ValidationResult.Success;
        }
        catch (Exception ex)
        {
            return new ValidationResult($"JSON 序列化失败: {ex.Message}");
        }
    }
}
