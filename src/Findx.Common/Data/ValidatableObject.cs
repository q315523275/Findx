using System.ComponentModel.DataAnnotations;

namespace Findx.Data;

/// <summary>
///     对象验证基类
/// </summary>
public abstract class ValidatableObject
{
    /// <summary>
    ///     是否验证通过
    /// </summary>
    /// <returns></returns>
    public virtual bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    ///     验证并返回不通过集合
    /// </summary>
    /// <returns></returns>
    public virtual IList<ValidationResult> Validate()
    {
        IList<ValidationResult> validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(this, new ValidationContext(this, null, null), validationResults, true);
        return validationResults;
    }
}