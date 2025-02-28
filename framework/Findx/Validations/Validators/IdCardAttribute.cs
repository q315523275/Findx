﻿using Findx.Extensions;
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
        if (ErrorMessage == null && ErrorMessageResourceName == null)
            ErrorMessage = ErrorMsg;
        return base.FormatErrorMessage(name);
    }

    /// <summary>
    ///     是否验证通过
    /// </summary>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value.SafeString().IsNullOrWhiteSpace())
            return ValidationResult.Success;
        if (RegexUtility.IsMatch(value.SafeString(), ValidatePattern.IdCardPattern))
            return ValidationResult.Success;
        return new ValidationResult(FormatErrorMessage((string.IsNullOrWhiteSpace(validationContext.DisplayName)
            ? validationContext.MemberName
            : validationContext.DisplayName) ?? string.Empty));
    }
}