using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Findx.Common;
using Findx.Utilities;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 字符串
/// </summary>
public static partial class Extensions
{
    #region 字符判断：Null、空、全英文、包含...

    /// <summary>
    ///     字符串是否为空
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    ///     字符串是否为空
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    /// <summary>
    ///     是否英文字符串
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool IsEnglish(this string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return false;
        var bytes = Encoding.UTF8.GetBytes(content);
        return bytes.Length == content.Length;
    }

    /// <summary>
    ///     判断字符串是否包含字符
    /// </summary>
    /// <param name="source"></param>
    /// <param name="check"></param>
    /// <param name="comp"></param>
    /// <returns></returns>
    public static bool Contains(this string source, string check, StringComparison comp)
    {
        return source.IndexOf(check, comp) >= 0;
    }

    /// <summary>
    ///     是否电子邮件
    /// </summary>
    public static bool IsEmail(this string value)
    {
        const string pattern = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
        return value.IsMatch(pattern);
    }

    /// <summary>
    ///     是否是IP地址
    /// </summary>
    public static bool IsIpAddress(this string value)
    {
        const string pattern =
            @"^((?:(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(?:25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d))))$";
        return value.IsMatch(pattern);
    }

    /// <summary>
    ///     是否是整数
    /// </summary>
    public static bool IsNumeric(this string value)
    {
        const string pattern = @"^\-?[0-9]+$";
        return value.IsMatch(pattern);
    }

    /// <summary>
    ///     是否是Unicode字符串
    /// </summary>
    public static bool IsUnicode(this string value)
    {
        const string pattern = @"^[\u4E00-\u9FA5\uE815-\uFA29]+$";
        return value.IsMatch(pattern);
    }

    /// <summary>
    ///     是否Url字符串
    /// </summary>
    public static bool IsUrl(this string value)
    {
        try
        {
            if (value.IsNullOrEmpty() || value.Contains(' ')) return false;
            _ = new Uri(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     是否身份证号，验证如下3种情况：
    ///     1.身份证号码为15位数字；
    ///     2.身份证号码为18位数字；
    ///     3.身份证号码为17位数字+1个字母
    /// </summary>
    public static bool IsIdentityCardId(this string value)
    {
        if (value.Length != 15 && value.Length != 18) return false;
        Regex regex;
        string[] array;
        if (value.Length == 15)
        {
            regex = new Regex(@"^(\d{6})(\d{2})(\d{2})(\d{2})(\d{3})_");
            if (!regex.Match(value).Success) return false;
            array = regex.Split(value);
            return DateTime.TryParse($"{"19" + array[2]}-{array[3]}-{array[4]}", out _);
        }

        regex = new Regex(@"^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9Xx])$");
        if (!regex.Match(value).Success) return false;
        array = regex.Split(value);
        if (!DateTime.TryParse($"{array[2]}-{array[3]}-{array[4]}", out _)) return false;
        // 校验最后一位
        var chars = value.ToCharArray().Select(m => m.ToString()).ToArray();
        int[] weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            var num = int.Parse(chars[i]);
            sum += num * weights[i];
        }

        var mod = sum % 11;
        var vCode = "10X98765432"; // 检验码字符串
        var last = vCode.ToCharArray().ElementAt(mod).ToString();
        return chars.Last().ToUpper() == last;
    }

    /// <summary>
    ///     是否手机号码
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isRestrict">是否按严格格式验证</param>
    public static bool IsMobileNumber(this string value, bool isRestrict = false)
    {
        var pattern = isRestrict ? @"^[1][3-9]\d{9}$" : @"^[1]\d{10}$";
        return value.IsMatch(pattern);
    }

    /// <summary>
    ///     判断指定路径是否图片文件
    /// </summary>
    public static bool IsImageFile(this string filename)
    {
        if (!File.Exists(filename)) return false;
        var fileData = File.ReadAllBytes(filename);
        if (fileData.Length == 0) return false;
        var code = BitConverter.ToUInt16(fileData, 0);
        switch (code)
        {
            case 0x4D42: // bmp
            case 0xD8FF: // jpg
            case 0x4947: // gif
            case 0x5089: // png
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region 字符处理：截取、删除、替换、分割、追加...

    /// <summary>
    ///     字符串截取，,从指定索引位置开始
    /// </summary>
    /// <param name="str"></param>
    /// <param name="startIndex"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string Sub(this string str, int startIndex, int len)
    {
        Check.NotNull(str, nameof(str));

        if (str.Length < startIndex + len) throw new ArgumentException($"字符串长度小于{startIndex + len}");

        return str.Substring(startIndex, len);
    }

    /// <summary>
    ///     字符串截取,从0索引位置开始
    /// </summary>
    /// <param name="str"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public static string Left(this string str, int len)
    {
        Check.NotNull(str, nameof(str));

        if (str.Length < len) throw new ArgumentException($"字符串长度小于{len}");

        return str.Substring(0, len);
    }

    /// <summary>
    ///     字符串截取,从末尾截取指定长度
    /// </summary>
    /// <param name="str"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public static string Right(this string str, int len)
    {
        Check.NotNull(str, nameof(str));

        if (str.Length < len) throw new ArgumentException($"字符串长度小于{len}");

        return str.Substring(str.Length - len, len);
    }

    /// <summary>
    ///     获取字符串中第n个出现char的索引位置
    /// </summary>
    /// <param name="str"></param>
    /// <param name="c"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static int NthIndexOf(this string str, char c, int n)
    {
        Check.NotNull(str, nameof(str));

        var count = 0;
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] != c) continue;

            if (++count == n) return i;
        }

        return -1;
    }

    /// <summary>
    ///     从字符串的末尾删除匹配后缀
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="postFixes">one or more postfix.</param>
    /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
    public static string RemovePostFix(this string str, params string[] postFixes)
    {
        return str.RemovePostFix(StringComparison.Ordinal, postFixes);
    }

    /// <summary>
    ///     从字符串的末尾删除匹配后缀
    /// </summary>
    /// <param name="str"></param>
    /// <param name="comparisonType"></param>
    /// <param name="postFixes"></param>
    /// <returns></returns>
    public static string RemovePostFix(this string str, StringComparison comparisonType, params string[] postFixes)
    {
        if (str.IsNullOrEmpty()) return null;

        if (postFixes.IsNullOrEmpty()) return str;

        foreach (var postFix in postFixes)
            if (str.EndsWith(postFix, comparisonType))
                return str.Left(str.Length - postFix.Length);

        return str;
    }

    /// <summary>
    ///     从字符串的头部删除匹配后缀
    /// </summary>
    /// <param name="str"></param>
    /// <param name="preFixes"></param>
    /// <returns></returns>
    public static string RemovePreFix(this string str, params string[] preFixes)
    {
        return str.RemovePreFix(StringComparison.Ordinal, preFixes);
    }

    /// <summary>
    ///     从字符串的头部删除匹配后缀
    /// </summary>
    /// <param name="str"></param>
    /// <param name="comparisonType"></param>
    /// <param name="preFixes"></param>
    /// <returns></returns>
    public static string RemovePreFix(this string str, StringComparison comparisonType, params string[] preFixes)
    {
        if (str.IsNullOrEmpty()) return null;

        if (preFixes.IsNullOrEmpty()) return str;

        foreach (var preFix in preFixes)
            if (str.StartsWith(preFix, comparisonType))
                return str.Right(str.Length - preFix.Length);

        return str;
    }

    /// <summary>
    ///     替换匹配字符
    /// </summary>
    /// <param name="str"></param>
    /// <param name="search"></param>
    /// <param name="replace"></param>
    /// <param name="comparisonType"></param>
    /// <returns></returns>
    public static string ReplaceFirst(this string str, string search, string replace,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        Check.NotNull(str, nameof(str));

        var pos = str.IndexOf(search, comparisonType);
        if (pos < 0) return str;

        return str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
    }

    /// <summary>
    ///     分割字符串
    /// </summary>
    public static string[] Split(this string str, string separator)
    {
        return str.Split(new[] { separator }, StringSplitOptions.None);
    }

    /// <summary>
    ///     分割字符串
    /// </summary>
    public static string[] Split(this string str, string separator, StringSplitOptions options)
    {
        return str.Split(new[] { separator }, options);
    }

    /// <summary>
    ///     通过换行符分割字符串
    /// </summary>
    public static string[] SplitToLines(this string str)
    {
        return str.Split(Environment.NewLine);
    }

    /// <summary>
    ///     通过换行符分割字符串
    /// </summary>
    public static string[] SplitToLines(this string str, StringSplitOptions options)
    {
        return str.Split(Environment.NewLine, options);
    }

    /// <summary>
    ///     以参数c为结尾，不是则进行追加
    /// </summary>
    /// <param name="str"></param>
    /// <param name="c"></param>
    /// <param name="comparisonType"></param>
    /// <returns></returns>
    public static string EnsureEndsWith(this string str, char c,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        Check.NotNull(str, nameof(str));

        if (str.EndsWith(c.ToString(), comparisonType)) return str;

        return str + c;
    }

    /// <summary>
    ///     以参数c为开始，不是则进行追加
    /// </summary>
    /// <param name="str"></param>
    /// <param name="c"></param>
    /// <param name="comparisonType"></param>
    /// <returns></returns>
    public static string EnsureStartsWith(this string str, char c,
        StringComparison comparisonType = StringComparison.Ordinal)
    {
        Check.NotNull(str, nameof(str));

        if (str.StartsWith(c.ToString(), comparisonType)) return str;

        return c + str;
    }

    /// <summary>
    ///     字符串截取,LEFT
    /// </summary>
    /// <param name="str"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Truncate(this string str, int maxLength)
    {
        if (str == null) return null;

        if (str.Length <= maxLength) return str;

        return str.Left(maxLength);
    }

    /// <summary>
    ///     字符串截取,RIGHT
    /// </summary>
    /// <param name="str"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string TruncateFromBeginning(this string str, int maxLength)
    {
        if (str == null) return null;

        if (str.Length <= maxLength) return str;

        return str.Right(maxLength);
    }

    /// <summary>
    ///     字符串超过最大长度，截取后使用指定后缀进行拼接
    /// </summary>
    /// <param name="str"></param>
    /// <param name="maxLength"></param>
    /// <param name="postfix"></param>
    /// <returns></returns>
    public static string TruncateWithPostfix(this string str, int maxLength, string postfix = "...")
    {
        if (str == null) return null;

        if (str == string.Empty || maxLength == 0) return string.Empty;

        if (str.Length <= maxLength) return str;

        if (maxLength <= postfix.Length) return postfix.Left(maxLength);

        return str.Left(maxLength - postfix.Length) + postfix;
    }

    /// <summary>
    ///     将字符串反转
    /// </summary>
    /// <param name="value">要反转的字符串</param>
    public static string ReverseString(this string value)
    {
        Check.NotNullOrWhiteSpace(value, "value");
        return new string(value.Reverse().ToArray());
    }

    /// <summary>
    ///     支持汉字的字符串长度，汉字长度计为2
    /// </summary>
    /// <param name="value">参数字符串</param>
    /// <returns>当前字符串的长度，汉字长度为2</returns>
    public static int TextLength(this string value)
    {
        var ascii = new ASCIIEncoding();
        var tempLen = 0;
        var bytes = ascii.GetBytes(value);
        foreach (var b in bytes)
            if (b == 63)
                tempLen += 2;
            else
                tempLen += 1;
        return tempLen;
    }

    #endregion

    #region 字符格式化：目录字符、换行符...

    /// <summary>
    ///     格式化目录地址
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string NormalizePath(this string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (Path.DirectorySeparatorChar == '\\')
            path = path.Replace('/', Path.DirectorySeparatorChar);
        else if (Path.DirectorySeparatorChar == '/')
            path = path.Replace('\\', Path.DirectorySeparatorChar);

        return path;
    }

    /// <summary>
    ///     格式化换行符
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string NormalizeLineEndings(this string str)
    {
        return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
    }

    #endregion

    #region 字符串转换：枚举、MD5、Byte、驼峰、大小写...

    /// <summary>
    ///     字符串转换为枚举
    /// </summary>
    /// <typeparam name="T">Type of enum</typeparam>
    /// <param name="value">String value to convert</param>
    /// <returns>Returns enum object</returns>
    public static T ToEnum<T>(this string value)
        where T : struct
    {
        Check.NotNull(value, nameof(value));
        return (T)Enum.Parse(typeof(T), value);
    }

    /// <summary>
    ///     字符串转换为枚举
    /// </summary>
    /// <typeparam name="T">Type of enum</typeparam>
    /// <param name="value">String value to convert</param>
    /// <param name="ignoreCase">Ignore case</param>
    /// <returns>Returns enum object</returns>
    public static T ToEnum<T>(this string value, bool ignoreCase)
        where T : struct
    {
        Check.NotNull(value, nameof(value));
        return (T)Enum.Parse(typeof(T), value, ignoreCase);
    }

    /// <summary>
    ///     字符串MD5加密
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToMd5(this string str)
    {
        return EncryptUtility.Md5By32(str);
    }

    /// <summary>
    ///     字符串转换为小驼峰字符串
    ///     <para>例如: "ThisIsSampleSentence" 转换为 "thisIsSampleSentence".</para>
    /// </summary>
    /// <param name="str"></param>
    /// <param name="useCurrentCulture"></param>
    /// <returns></returns>
    public static string ToCamelCase(this string str, bool useCurrentCulture = false)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        if (str.Length == 1) return useCurrentCulture ? str.ToLower() : str.ToLowerInvariant();

        return (useCurrentCulture ? char.ToLower(str[0]) : char.ToLowerInvariant(str[0])) + str.Substring(1);
    }

    /// <summary>
    ///     将组合单词的字符串转换为空格分开的字符串
    ///     <para>例如: "ThisIsSampleSentence" 转换为 "This is a sample sentence".</para>
    /// </summary>
    /// <param name="str"></param>
    /// <param name="useCurrentCulture"></param>
    /// <returns></returns>
    public static string ToSentenceCase(this string str, bool useCurrentCulture = false)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return useCurrentCulture
            ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]))
            : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLowerInvariant(m.Value[1]));
    }

    /// <summary>
    ///     将组合单词的字符串转换为横线连接的字符串
    ///     <para>例如: "ThisIsSampleSentence" 换换为 "This-is-a-sample-sentence".</para>
    /// </summary>
    /// <param name="str"></param>
    /// <param name="useCurrentCulture"></param>
    /// <returns></returns>
    public static string ToKebabCase(this string str, bool useCurrentCulture = false)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        str = str.ToCamelCase();

        return useCurrentCulture
            ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + "-" + char.ToLower(m.Value[1]))
            : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + "-" + char.ToLowerInvariant(m.Value[1]));
    }

    /// <summary>
    ///     将组合单词的字符串转换为横线连接的字符串
    ///     <para>例如: "ThisIsSampleSentence" 换换为 "This_is_a_sample_sentence".</para>
    /// </summary>
    /// <param name="str">String to convert.</param>
    /// <returns></returns>
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        var builder = new StringBuilder(str.Length + Math.Min(2, str.Length / 5));
        var previousCategory = default(UnicodeCategory?);

        for (var currentIndex = 0; currentIndex < str.Length; currentIndex++)
        {
            var currentChar = str[currentIndex];
            if (currentChar == '_')
            {
                builder.Append('_');
                previousCategory = null;
                continue;
            }

            var currentCategory = char.GetUnicodeCategory(currentChar);
            switch (currentCategory)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == UnicodeCategory.SpaceSeparator ||
                        previousCategory == UnicodeCategory.LowercaseLetter ||
                        (previousCategory != UnicodeCategory.DecimalDigitNumber &&
                         previousCategory != null &&
                         currentIndex > 0 &&
                         currentIndex + 1 < str.Length &&
                         char.IsLower(str[currentIndex + 1])))
                        builder.Append('_');

                    currentChar = char.ToLower(currentChar);
                    break;

                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == UnicodeCategory.SpaceSeparator) builder.Append('_');
                    break;

                default:
                    if (previousCategory != null) previousCategory = UnicodeCategory.SpaceSeparator;
                    continue;
            }

            builder.Append(currentChar);
            previousCategory = currentCategory;
        }

        return builder.ToString();
    }

    /// <summary>
    ///     字符串转换为大驼峰
    /// </summary>
    /// <param name="str"></param>
    /// <param name="useCurrentCulture"></param>
    /// <returns></returns>
    public static string ToPascalCase(this string str, bool useCurrentCulture = false)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        if (str.Length == 1) return useCurrentCulture ? str.ToUpper() : str.ToUpperInvariant();

        return (useCurrentCulture ? char.ToUpper(str[0]) : char.ToUpperInvariant(str[0])) + str.Substring(1);
    }

    /// <summary>
    ///     将字符串转换为<see cref="byte" />[]数组，默认编码为<see cref="Encoding.UTF8" />
    /// </summary>
    public static byte[] ToBytes(this string value, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetBytes(value);
    }

    /// <summary>
    ///     将<see cref="byte" />[]数组转换为字符串，默认编码为<see cref="Encoding.UTF8" />
    /// </summary>
    public static string ToString2(this byte[] bytes, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return encoding.GetString(bytes);
    }

    /// <summary>
    ///     将<see cref="byte" />[]数组转换为Base64字符串
    /// </summary>
    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    ///     将字符串转换为Base64字符串，默认编码为<see cref="Encoding.UTF8" />
    /// </summary>
    /// <param name="source">正常的字符串</param>
    /// <param name="encoding">编码</param>
    /// <returns>Base64字符串</returns>
    public static string ToBase64String(this string source, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return Convert.ToBase64String(encoding.GetBytes(source));
    }

    /// <summary>
    ///     将Base64字符串转换为正常字符串，默认编码为<see cref="Encoding.UTF8" />
    /// </summary>
    /// <param name="base64String">Base64字符串</param>
    /// <param name="encoding">编码</param>
    /// <returns>正常字符串</returns>
    public static string FromBase64String(this string base64String, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = Convert.FromBase64String(base64String);
        return encoding.GetString(bytes);
    }

    /// <summary>
    ///     将字符串进行UrlDecode解码
    /// </summary>
    /// <param name="source">待UrlDecode解码的字符串</param>
    /// <returns>UrlDecode解码后的字符串</returns>
    public static string ToUrlDecode(this string source)
    {
        return HttpUtility.UrlDecode(source);
    }

    /// <summary>
    ///     将字符串进行UrlEncode编码
    /// </summary>
    /// <param name="source">待UrlEncode编码的字符串</param>
    /// <returns>UrlEncode编码后的字符串</returns>
    public static string ToUrlEncode(this string source)
    {
        return HttpUtility.UrlEncode(source);
    }

    /// <summary>
    ///     将字符串进行HtmlDecode解码
    /// </summary>
    /// <param name="source">待HtmlDecode解码的字符串</param>
    /// <returns>HtmlDecode解码后的字符串</returns>
    public static string ToHtmlDecode(this string source)
    {
        return HttpUtility.HtmlDecode(source);
    }

    /// <summary>
    ///     将字符串进行HtmlEncode编码
    /// </summary>
    /// <param name="source">待HtmlEncode编码的字符串</param>
    /// <returns>HtmlEncode编码后的字符串</returns>
    public static string ToHtmlEncode(this string source)
    {
        return HttpUtility.HtmlEncode(source);
    }

    /// <summary>
    ///     将字符串转换为十六进制字符串，默认编码为<see cref="Encoding.UTF8" />
    /// </summary>
    public static string ToHexString(this string source, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(source);
        return bytes.ToHexString();
    }

    /// <summary>
    ///     将十六进制字符串转换为常规字符串，默认编码为<see cref="Encoding.UTF8" />
    /// </summary>
    public static string FromHexString(this string hexString, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = hexString.ToHexBytes();
        return encoding.GetString(bytes);
    }

    /// <summary>
    ///     将byte[]编码为十六进制字符串
    /// </summary>
    /// <param name="bytes">byte[]数组</param>
    /// <returns>十六进制字符串</returns>
    public static string ToHexString(this byte[] bytes)
    {
        return EncryptUtility.ToHexString(bytes);
    }

    /// <summary>
    ///     将十六进制字符串转换为byte[]
    /// </summary>
    /// <param name="hexString">十六进制字符串</param>
    /// <returns>byte[]数组</returns>
    public static byte[] ToHexBytes(this string hexString)
    {
        return EncryptUtility.FromHexString(hexString);
    }

    /// <summary>
    ///     将字符串进行Unicode编码，变成形如“\u7f16\u7801”的形式
    /// </summary>
    /// <param name="source">要进行编号的字符串</param>
    public static string ToUnicodeString(this string source)
    {
        var regex = new Regex(@"[^\u0000-\u00ff]");
        return regex.Replace(source, m => $@"\u{(short)m.Value[0]:x4}");
    }

    /// <summary>
    ///     将形如“\u7f16\u7801”的Unicode字符串解码
    /// </summary>
    public static string FromUnicodeString(this string source)
    {
        var regex = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);
        return regex.Replace(source,
            m =>
            {
                if (short.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InstalledUICulture, out var s)) 
                    return "" + (char)s;
                
                return m.Value;
            });
    }

    #endregion

    #region 正则表达式

    /// <summary>
    ///     指示所指定的正则表达式在指定的输入字符串中是否找到了匹配项
    /// </summary>
    /// <param name="value">要搜索匹配项的字符串</param>
    /// <param name="pattern">要匹配的正则表达式模式</param>
    /// <param name="isContains">是否包含，否则全匹配</param>
    /// <returns>如果正则表达式找到匹配项，则为 true；否则，为 false</returns>
    public static bool IsMatch(this string value, string pattern, bool isContains = true)
    {
        if (value == null) return false;
        return isContains ? Regex.IsMatch(value, pattern) : Regex.Match(value, pattern).Success;
    }

    /// <summary>
    ///     在指定的输入字符串中搜索指定的正则表达式的第一个匹配项
    /// </summary>
    /// <param name="value">要搜索匹配项的字符串</param>
    /// <param name="pattern">要匹配的正则表达式模式</param>
    /// <returns>一个对象，包含有关匹配项的信息</returns>
    public static string Match(this string value, string pattern)
    {
        if (value == null) return null;
        return Regex.Match(value, pattern).Value;
    }

    /// <summary>
    ///     在指定的输入字符串中匹配并替换符合指定正则表达式的子串
    /// </summary>
    public static string ReplaceRegex(this string value, string pattern, string replacement)
    {
        if (value == null) return null;

        return Regex.Replace(value, pattern, replacement);
    }

    /// <summary>
    ///     在指定的输入字符串中搜索指定的正则表达式的所有匹配项的字符串集合
    /// </summary>
    /// <param name="value"> 要搜索匹配项的字符串 </param>
    /// <param name="pattern"> 要匹配的正则表达式模式 </param>
    /// <returns> 一个集合，包含有关匹配项的字符串值 </returns>
    public static IEnumerable<string> Matches(this string value, string pattern)
    {
        if (value == null) return new string[] { };
        var matches = Regex.Matches(value, pattern);
        return from Match match in matches select match.Value;
    }

    /// <summary>
    ///     在指定的输入字符串中匹配第一个数字字符串
    /// </summary>
    public static string MatchFirstNumber(this string value)
    {
        var matches = Regex.Matches(value, @"\d+");
        return matches.Count == 0 ? string.Empty : matches[0].Value;
    }

    /// <summary>
    ///     在指定字符串中匹配最后一个数字字符串
    /// </summary>
    public static string MatchLastNumber(this string value)
    {
        var matches = Regex.Matches(value, @"\d+");
        return matches.Count == 0 ? string.Empty : matches[^1].Value;
    }

    /// <summary>
    ///     在指定字符串中匹配所有数字字符串
    /// </summary>
    public static IEnumerable<string> MatchNumbers(this string value)
    {
        return Matches(value, @"\d+");
    }

    /// <summary>
    ///     检测指定字符串中是否包含数字
    /// </summary>
    public static bool IsMatchNumber(this string value)
    {
        return IsMatch(value, @"\d");
    }

    /// <summary>
    ///     检测指定字符串是否全部为数字并且长度等于指定长度
    /// </summary>
    public static bool IsMatchNumber(this string value, int length)
    {
        var regex = new Regex(@"^\d{" + length + "}$");
        return regex.IsMatch(value);
    }

    #endregion
}