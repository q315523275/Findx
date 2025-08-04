namespace System.ComponentModel.DataAnnotations;

/// <summary>
///     自定义允许内容
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CustomAllowedValuesAttribute: ValidationAttribute
{
    private readonly string[] _allowedValues;

    /// <summary>
    ///     接受允许的值列表
    /// </summary>
    /// <param name="allowedValues"></param>
    public CustomAllowedValuesAttribute(params string[] allowedValues)
    {
        _allowedValues = allowedValues;
    }

    /// <summary>
    ///     验证
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // 允许 null 值（可根据需求调整）
        }

        var input = value.ToString()?.Trim();
        if (_allowedValues.Contains(input, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? $"值 {input} 不在允许的范围内。");
    }
}