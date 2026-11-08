using Findx.Extensions;
using Findx.Utilities;
using Findx.Validations.Validators;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
///     中文验证
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class ChineseAttribute : ValidationAttribute
{
    /// <summary>
    ///     错误消息
    /// </summary>
    private const string ErrorMsg = "'{0}' 必须是中文";

    /// <summary>
    ///     格式化错误消息
    /// </summary>
    public override string FormatErrorMessage(string name)
    {
        //  不在反射缓存的共享特性实例上写回 ErrorMessage（避免并发校验时修改共享状态）；
        //  用户未自定义错误消息/资源时本地格式化默认消息，输出与原先一致
        if (ErrorMessage == null && ErrorMessageResourceName == null)
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, ErrorMsg, name);
        return base.FormatErrorMessage(name);
    }

    /// <summary>
    ///     是否验证通过
    /// </summary>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value.SafeString().IsNullOrWhiteSpace())
            return ValidationResult.Success;
        if (RegexUtility.IsMatch(value.SafeString(), ValidatePattern.ChinesePattern))
            return ValidationResult.Success;
        return new ValidationResult(FormatErrorMessage((string.IsNullOrWhiteSpace(validationContext.DisplayName)
            ? validationContext.MemberName
            : validationContext.DisplayName) ?? string.Empty));
    }
}