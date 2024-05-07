using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     数据脱敏工具类
/// </summary>
public static class DesensitizedUtility
{
    /// <summary>
    ///     定义了一个first_mask的规则，只显示第一个字符，其他字符都用*代替
    ///     脱敏前：123456789；脱敏后：1********。
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string FirstMask(string str)
    {
        return string.IsNullOrWhiteSpace(str) ? string.Empty : Replace(str, 1, str.Length);
    }

    /// <summary>
    ///     替换字符串指定位置字符为指定字符
    /// </summary>
    /// <param name="str"></param>
    /// <param name="startInclude"></param>
    /// <param name="endExclude"></param>
    /// <param name="replacedChar"></param>
    /// <returns></returns>
    public static string Replace(string str, int startInclude, int endExclude, char replacedChar = '*')
    {
        if (string.IsNullOrWhiteSpace(str)) 
            return string.Empty;
        
        var strLength = str.Length;
        var strCodePoints = str.ToArray();
        
        if (startInclude > strLength) 
        {
            return str;
        }
        
        if (endExclude > strLength) 
        {
            endExclude = strLength;
        }
        
        if (startInclude > endExclude) 
        {
            // 如果起始位置大于结束位置，不替换
            return str;
        }

        using var psb = Pool.StringBuilder.Get(out var sb);
        for (var i = 0; i < strLength; i++) 
        {
            if (i >= startInclude && i < endExclude) 
                sb.Append(replacedChar);
            else 
                sb.Append(strCodePoints[i]);
        }
        return sb.ToString();
    }

    /// <summary>
    ///     【中文姓名】只显示第一个汉字，其他隐藏为2个星号，比如：李**
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public static string ChineseName(string fullName)
    {
        return FirstMask(fullName);
    }
    
    /// <summary>
    ///    【身份证号】显示前六位，后四位，其他隐藏。共计18位或者15位，比如：340304********1234
    /// </summary>
    /// <param name="idNo"></param>
    /// <param name="front"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string IdCard(string idNo, int front, int end) 
    {
        // 身份证不能为空
        if (string.IsNullOrWhiteSpace(idNo)) 
            return string.Empty;
        
        // 需要截取的长度不能大于身份证号长度
        if ((front + end) > idNo.Length) 
            return string.Empty;
        
        // 需要截取的不能小于0
        if (front < 0 || end < 0)
            return string.Empty;
        
        return Replace(idNo, front, idNo.Length - end);
    }
    
    /// <summary>
    ///     【手机号码】前三位，后4位，其他隐藏，比如135****2210
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    public static string MobilePhone(string phone)
    {
        return string.IsNullOrWhiteSpace(phone) ? string.Empty : Replace(phone, 3, phone.Length - 4);
    }

    /// <summary>
    ///     【中国车牌】车牌中间用*代替，比如：苏D4***0
    /// </summary>
    /// <param name="carLicense"></param>
    /// <returns></returns>
    public static string CarLicense(string carLicense) 
    {
        if (string.IsNullOrWhiteSpace(carLicense)) 
            return string.Empty;

        carLicense = carLicense.Length switch
        {
            // 普通车牌
            7 => Replace(carLicense, 3, 6),
            8 => Replace(carLicense, 3, 7),
            _ => carLicense
        };
        return carLicense;
    }
    
    /// <summary>
    ///     【银行卡号脱敏】由于银行卡号长度不定，所以只展示前4位，后面的位数根据卡号决定展示1-4位，比如：6222 ****** 1234
    /// </summary>
    /// <param name="bankCardNo"></param>
    /// <returns></returns>
    public static string BankCard(string bankCardNo) 
    {
        if (string.IsNullOrWhiteSpace(bankCardNo)) 
            return string.Empty;
        
        if (bankCardNo.Length < 9) 
            return bankCardNo;

        var length = bankCardNo.Length;
        var endLength= length % 4 == 0 ? 4 : length % 4;
        var midLength = length - 4 - endLength;

        using var psb = Pool.StringBuilder.Get(out var sb);
        sb.Append(bankCardNo, 0, 4);
        for (var i = 0; i < midLength; ++i) 
        {
            if (i % 4 == 0) 
                sb.Append(' ');
            sb.Append('*');
        }
        sb.Append(' ').Append(bankCardNo, length - endLength, length);
        return sb.ToString();
    }
}