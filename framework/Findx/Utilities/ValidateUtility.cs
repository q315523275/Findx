using System.ComponentModel.DataAnnotations;
using Findx.Validations;

namespace Findx.Utilities;

/// <summary>
///     验证操作
/// </summary>
public static class ValidateUtility
{
    /// <summary>
    ///     模型校验
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ValidResult IsValid(object value)
    {
        var result = new ValidResult();
        try
        {
            var validationContext = new ValidationContext(value);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(value, validationContext, results, true);

            if (!isValid)
            {
                result.IsVaild = false;
                result.ErrorMembers = new List<ErrorMember>();
                foreach (var item in results)
                    result.ErrorMembers.Add(new ErrorMember
                    {
                        ErrorMessage = item.ErrorMessage,
                        ErrorMemberName = item.MemberNames.FirstOrDefault()
                    });
            }
            else
            {
                result.IsVaild = true;
            }
        }
        catch (Exception ex)
        {
            result.IsVaild = false;
            result.ErrorMembers = new List<ErrorMember>
            {
                new()
                {
                    ErrorMessage = ex.Message,
                    ErrorMemberName = "Internal error"
                }
            };
        }

        return result;
    }
}