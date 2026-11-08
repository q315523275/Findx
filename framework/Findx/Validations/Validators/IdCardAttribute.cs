using Findx.Extensions;
using Findx.Utilities;
using Findx.Validations.Validators;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
///     身份证验证
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class IdCardAttribute : ValidationAttribute
{
    /// <summary>
    ///     错误消息
    /// </summary>
    private const string ErrorMsg = "'{0}' 不是有效的身份证号";

    /// <summary>
    ///     格式化错误消息
    /// </summary>
    public override string FormatErrorMessage(string name)
    {
        //  不在反射缓存的共享特性实例上写回 ErrorMessage（避免并发校验时修改共享状态）；
        //  用户未自定义错误消息/资源时本地格式化默认消息，输出与原先一致
        if (ErrorMessage == null && ErrorMessageResourceName == null)
            return string.Format(Globalization.CultureInfo.CurrentCulture, ErrorMsg, name);
        return base.FormatErrorMessage(name);
    }

    /// <summary>
    ///     是否验证通过
    /// </summary>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value.SafeString().IsNullOrWhiteSpace())
            return ValidationResult.Success;

        var input = value.SafeString();
        if (RegexUtility.IsMatch(input, ValidatePattern.IdCardPattern) && IsValidIdCard(input))
            return ValidationResult.Success;

        var displayName = validationContext?.DisplayName ?? string.Empty;
        var memberName = validationContext?.MemberName ?? string.Empty;
        var name = (string.IsNullOrWhiteSpace(displayName) ? memberName : displayName) ?? string.Empty;
        return new ValidationResult(FormatErrorMessage(name));
    }

    /// <summary>
    ///     校验身份证实质合法性：出生日期为有效日历日，18 位号码校验位符合 ISO 7064 MOD 11-2
    /// </summary>
    /// <param name="idCard"></param>
    /// <returns></returns>
    private static bool IsValidIdCard(string idCard)
    {
        //  15 位老身份证的出生年份按 19xx 处理
        var birthPart = idCard.Length == 15 ? $"19{idCard.Substring(6, 6)}" : idCard.Substring(6, 8);
        if (!DateTime.TryParseExact(birthPart, "yyyyMMdd", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, out _))
            return false;

        //  18 位：前 17 位分别乘权重后求和，对 11 取模映射校验位
        if (idCard.Length == 15) return true;

        int[] weights = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];
        char[] checkCodes = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];
        var sum = 0;
        for (var i = 0; i < 17; i++) sum += (idCard[i] - '0') * weights[i];
        return char.ToUpperInvariant(idCard[17]) == checkCodes[sum % 11];
    }
}