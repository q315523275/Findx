using Findx.Extensions;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
///     集合大小验证
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class CollectionSizeAttribute: ValidationAttribute
{
    /// <summary>
    ///     最小值
    /// </summary>
    public int Min { get; set; }
    
    /// <summary>
    ///     最大值
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    ///     是否验证通过
    /// </summary>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not ICollection collection) 
            return ValidationResult.Success;
        
        var count = collection.Count;
        if (count < Min || (Max > 0 && count > Max))
        {
            var name = (validationContext.DisplayName.IsNullOrWhiteSpace() ? validationContext.MemberName : validationContext.DisplayName) ?? string.Empty;
            return new ValidationResult($"{name} 数量必须在 {Min}和{Max} 之间");
        }
        
        return ValidationResult.Success;
    }
}