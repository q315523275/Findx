using System;
using System.Linq;
using System.Reflection;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 枚举
    /// </summary>
    public partial class Extensions
    {
        /// <summary>
        /// 获取枚举项上的<see cref="DescriptionAttribute"/>特性的文字描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum value)
        {
            Type type = value.GetType();
            MemberInfo member = type.GetMember(value.ToString()).FirstOrDefault();
            return member != null ? member.GetDescription() : value.ToString();
        }
    }
}
