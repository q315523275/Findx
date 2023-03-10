using Findx.Extensions;
using Findx.Validations.Validators;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Findx.Utils
{
    /// <summary>
    /// 社会信用代码工具
    /// <para></para>
    /// <para>第一部分：登记管理部门代码1位 (数字或大写英文字母)</para>
    /// <para>第二部分：机构类别代码1位(数字或大写英文字母)</para>
    /// <para>第三部分：登记管理机关行政区划码6位(数字)</para>
    /// <para>第四部分：主体标识码（组织机构代码）9位(数字或大写英文字母)</para>
    /// <para>第五部分：校验码1位(数字或大写英文字母)</para>
    /// </summary>
    public static class CreditCode
    {
        private static readonly int[] Weight = { 1, 3, 9, 27, 19, 26, 16, 17, 20, 29, 25, 13, 8, 24, 10, 30, 28 };
        // ReSharper disable once StringLiteralTypo
        private static readonly char[] BaseCodeArray = "0123456789ABCDEFGHJKLMNPQRTUWXY".ToCharArray();
        private static readonly IDictionary<char, int> CodeIndexMap = new Dictionary<char, int>();

        static CreditCode()
        {
            for (int i = 0; i < BaseCodeArray.Length; i++)
            {
                CodeIndexMap[BaseCodeArray[i]] = i;
            }
        }

        /// <summary>
        /// 正则校验统一社会信用代码（18位）
        /// </summary>
        /// <param name="creditCode"></param>
        /// <returns></returns>
        public static bool IsCreditCodeSimple(string creditCode)
        {
            if (creditCode.IsNullOrWhiteSpace())
            {
                return false;
            }
            return RegexUtil.IsMatch(creditCode, ValidatePattern.CreditCodePatter);
        }

        /// <summary>
        /// 是否是有效的统一社会信用代码
        /// </summary>
        /// <param name="creditCode"></param>
        /// <returns></returns>
        public static bool IsCreditCode(string creditCode)
        {
            if (!IsCreditCodeSimple(creditCode))
            {
                return false;
            }

            int parityBit = GetParityBit(creditCode);
            if (parityBit < 0)
            {
                return false;
            }

            return creditCode[17] == BaseCodeArray[parityBit];
        }

        /// <summary>
        /// 获取一个随机的统一社会信用代码
        /// </summary>
        /// <returns></returns>
        public static string RandomCreditCode()
        {
            StringBuilder buf = new StringBuilder(18);

            for (int i = 0; i < 2; i++)
            {
                int num = RandomUtil.RandomInt(BaseCodeArray.Length - 1);
                buf.Append(char.ToUpper(BaseCodeArray[num]));
            }
            for (int i = 2; i < 8; i++)
            {
                int num = RandomUtil.RandomInt(10);
                buf.Append(BaseCodeArray[num]);
            }
            for (int i = 8; i < 17; i++)
            {
                int num = RandomUtil.RandomInt(BaseCodeArray.Length - 1);
                buf.Append(BaseCodeArray[num]);
            }

            string code = buf.ToString();

            return code + BaseCodeArray[GetParityBit(code)];
        }


        private static int GetParityBit(string creditCode)
        {
            int sum = 0;
            int codeIndex;
            for (int i = 0; i < 17; i++)
            {
                if (!CodeIndexMap.ContainsKey(creditCode[i]))
                {
                    return -1;
                }
                codeIndex = CodeIndexMap[creditCode[i]];
                sum += codeIndex * Weight[i];
            }
            int result = 31 - sum % 31;
            return result == 31 ? 0 : result;
        }
    }
}
