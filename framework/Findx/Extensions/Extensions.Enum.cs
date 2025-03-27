using System.ComponentModel;
using Findx.Utilities;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 枚举
/// </summary>
public partial class Extensions
{
    /// <summary>
    ///     获取枚举项上的<see cref="DescriptionAttribute" />特性的文字描述
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToDescription<T>(this T value) where T: Enum
    {
        return EnumUtility<T>.GetDescription(value);
        
        // var type = value.GetType();
        // var member = type.GetMember(value.ToString()).FirstOrDefault();
        // return member != null ? member.GetDescription() : value.ToString();
    }
}