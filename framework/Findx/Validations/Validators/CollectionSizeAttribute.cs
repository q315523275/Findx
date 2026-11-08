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
        //  null 放行；字符串虽实现 IEnumerable，但按标量处理不参与集合数量校验（保持原行为）
        if (value is null or string)
            return ValidationResult.Success;

        if (value is not IEnumerable enumerable)
            return ValidationResult.Success;

        //  ICollection 直接取 Count；其余可枚举类型（IEnumerable<T>/IReadOnlyCollection<T> 等）枚举计数
        var count = enumerable is ICollection collection ? collection.Count : enumerable.Cast<object>().Count();

        if (count < Min || (Max > 0 && count > Max))
        {
            //  ValidationAttribute.IsValid(object) 单参重载（.NET 6+）传入的 validationContext 为 null，
            //  必须判空，否则 DisplayName 访问抛 NRE（ASP.NET 模型验证总会传 context，单参/手动调用场景无 context）
            var displayName = validationContext?.DisplayName;
            var memberName = validationContext?.MemberName ?? string.Empty;
            var name = string.IsNullOrWhiteSpace(displayName) ? memberName : displayName;
            var message = Max > 0 ? $"{name} 数量必须在 {Min} 和 {Max} 之间" : $"{name} 数量至少为 {Min} 个";
            return new ValidationResult(message);
        }

        return ValidationResult.Success;
    }
}