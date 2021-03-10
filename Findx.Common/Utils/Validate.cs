using Findx.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Findx.Utils
{
    /// <summary>
    /// 验证操作
    /// </summary>
    public class Validate
    {
        /// <summary>
        /// 模型校验
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ValidResult IsValid(object value)
        {
            ValidResult result = new ValidResult();
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
                    {
                        result.ErrorMembers.Add(new ErrorMember()
                        {
                            ErrorMessage = item.ErrorMessage,
                            ErrorMemberName = item.MemberNames.FirstOrDefault()
                        });
                    }
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
                    new ErrorMember()
                    {
                        ErrorMessage = ex.Message,
                        ErrorMemberName = "Internal error"
                    }
                };
            }

            return result;
        }
    }
}
