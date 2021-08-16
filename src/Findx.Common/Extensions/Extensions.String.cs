using Findx.Utils;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 字符串
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 以<param name="c"></param>为结尾，不是则进行追加
        /// </summary>
        /// <param name="str"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            Check.NotNull(str, nameof(str));

            if (str.EndsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// 以<param name="c"></param>为开始，不是则进行追加
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            Check.NotNull(str, nameof(str));

            if (str.StartsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return c + str;
        }

        /// <summary>
        /// 字符串是否为空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 字符串是否为空
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 字符串截取,从0索引位置开始
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string Left(this string str, int len)
        {
            Check.NotNull(str, nameof(str));

            if (str.Length < len)
            {
                throw new ArgumentException($"字符串长度小于{len}");
            }

            return str.Substring(0, len);
        }

        /// <summary>
        /// 格式化换行符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// 获取字符串中第n个出现char的索引位置
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
                if (str[i] != c)
                {
                    continue;
                }

                if ((++count) == n)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 从字符串的末尾删除匹配后缀
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="postFixes">one or more postfix.</param>
        /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            return str.RemovePostFix(StringComparison.Ordinal, postFixes);
        }

        /// <summary>
        /// 从字符串的末尾删除匹配后缀
        /// </summary>
        /// <param name="str"></param>
        /// <param name="comparisonType"></param>
        /// <param name="postFixes"></param>
        /// <returns></returns>
        public static string RemovePostFix(this string str, StringComparison comparisonType, params string[] postFixes)
        {
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (postFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix, comparisonType))
                {
                    return str.Left(str.Length - postFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// 从字符串的头部删除匹配后缀
        /// </summary>
        /// <param name="str"></param>
        /// <param name="preFixes"></param>
        /// <returns></returns>
        public static string RemovePreFix(this string str, params string[] preFixes)
        {
            return str.RemovePreFix(StringComparison.Ordinal, preFixes);
        }

        /// <summary>
        /// 从字符串的头部删除匹配后缀
        /// </summary>
        /// <param name="str"></param>
        /// <param name="comparisonType"></param>
        /// <param name="preFixes"></param>
        /// <returns></returns>
        public static string RemovePreFix(this string str, StringComparison comparisonType, params string[] preFixes)
        {
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            if (preFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix, comparisonType))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// 替换匹配字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string str, string search, string replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            Check.NotNull(str, nameof(str));

            var pos = str.IndexOf(search, comparisonType);
            if (pos < 0)
            {
                return str;
            }

            return str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
        }

        /// <summary>
        /// 字符串截取,从末尾截取指定长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string Right(this string str, int len)
        {
            Check.NotNull(str, nameof(str));

            if (str.Length < len)
            {
                throw new ArgumentException($"字符串长度小于{len}");
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        public static string[] Split(this string str, string separator)
        {
            return str.Split(new[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        public static string[] Split(this string str, string separator, StringSplitOptions options)
        {
            return str.Split(new[] { separator }, options);
        }

        /// <summary>
        /// 通过换行符分割字符串
        /// </summary>
        public static string[] SplitToLines(this string str)
        {
            return str.Split(Environment.NewLine);
        }

        /// <summary>
        /// 通过换行符分割字符串
        /// </summary>
        public static string[] SplitToLines(this string str, StringSplitOptions options)
        {
            return str.Split(Environment.NewLine, options);
        }

        /// <summary>
        /// 字符串转换为枚举
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
        /// 字符串转换为枚举
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
        /// 字符串MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMd5(this string str)
        {
            return Encrypt.Md5By32(str);
        }

        /// <summary>
        /// 字符串截取,LEFT
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Left(maxLength);
        }

        /// <summary>
        /// 字符串截取,RIGHT
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string TruncateFromBeginning(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Right(maxLength);
        }

        /// <summary>
        /// 字符串超过最大长度，截取后使用指定后缀进行拼接
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string TruncateWithPostfix(this string str, int maxLength)
        {
            return TruncateWithPostfix(str, maxLength, "...");
        }

        /// <summary>
        /// 字符串超过最大长度，截取后使用指定后缀进行拼接
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty || maxLength == 0)
            {
                return string.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            if (maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            return str.Left(maxLength - postfix.Length) + postfix;
        }

        /// <summary>
        /// 字符串转二进制流
        /// </summary>
        public static byte[] GetBytes(this string str)
        {
            return str.GetBytes(Encoding.UTF8);
        }

        /// <summary>
        /// 字符串转二进制流
        /// </summary>
        public static byte[] GetBytes(this string str, Encoding encoding)
        {
            Check.NotNull(str, nameof(str));
            Check.NotNull(encoding, nameof(encoding));

            return encoding.GetBytes(str);
        }

        /// <summary>
        /// 是否英文字符串
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool IsEnglish(this string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }
            var bytes = Encoding.UTF8.GetBytes(content);
            bool result = bytes.Length == content.Length;
            return result;
        }

        /// <summary>
        /// 判断字符串是否包含字符
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
        /// 字符串转换为小驼峰字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="useCurrentCulture"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return useCurrentCulture ? str.ToLower() : str.ToLowerInvariant();
            }

            return (useCurrentCulture ? char.ToLower(str[0]) : char.ToLowerInvariant(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// 将组合单词的字符串转换为空格分开的字符串
        /// 例如: "ThisIsSampleSentence" 转换为 "This is a sample sentence".
        /// </summary>
        /// <param name="str"></param>
        /// <param name="useCurrentCulture"></param>
        /// <returns></returns>
        public static string ToSentenceCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return useCurrentCulture
                ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]))
                : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLowerInvariant(m.Value[1]));
        }

        /// <summary>
        /// 将组合单词的字符串转换为横线连接的字符串
        /// 例如: "ThisIsSampleSentence" 换换为 "this-is-a-sample-sentence".
        /// </summary>
        /// <param name="str"></param>
        /// <param name="useCurrentCulture"></param>
        /// <returns></returns>
        public static string ToKebabCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            str = str.ToCamelCase();

            return useCurrentCulture
                ? Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + "-" + char.ToLower(m.Value[1]))
                : Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + "-" + char.ToLowerInvariant(m.Value[1]));
        }

        /// <summary>
        /// 将组合单词的字符串转换为横线连接的字符串
        /// 例如: "ThisIsSampleSentence" 换换为 "this_is_a_sample_sentence".
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <returns></returns>
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

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
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            previousCategory != null &&
                            currentIndex > 0 &&
                            currentIndex + 1 < str.Length &&
                            char.IsLower(str[currentIndex + 1]))
                        {
                            builder.Append('_');
                        }

                        currentChar = char.ToLower(currentChar);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            builder.Append('_');
                        }
                        break;

                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }

                builder.Append(currentChar);
                previousCategory = currentCategory;
            }

            return builder.ToString();
        }

        /// <summary>
        /// 字符串转换为大驼峰
        /// </summary>
        /// <param name="str"></param>
        /// <param name="useCurrentCulture"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str, bool useCurrentCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return useCurrentCulture ? str.ToUpper() : str.ToUpperInvariant();
            }

            return (useCurrentCulture ? char.ToUpper(str[0]) : char.ToUpperInvariant(str[0])) + str.Substring(1);
        }
    }
}
