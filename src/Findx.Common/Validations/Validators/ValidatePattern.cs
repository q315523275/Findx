namespace Findx.Validations.Validators
{
    /// <summary>
    /// 验证正则
    /// </summary>
    public static class ValidatePattern
    {
        /// <summary>
        /// 手机号验证正则表达式
        /// </summary>
        public const string MobilePhonePattern = "^1[0-9]{10}$";

        /// <summary>
        /// 身份证验证正则表达式
        /// </summary>
        public const string IdCardPattern = @"(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)";

        /// <summary>
        /// 中文验证正则表达式
        /// </summary>
        public const string ChinesePattern = @"^[\u4e00-\u9fa5]+$";

        /// <summary>
        /// Url验证正则表达式
        /// </summary>
        public const string UrlPattern =
            @"^http(s?):\/\/([\w\-]+(\.[\w\-]+)*\/)*[\w\-]+(\.[\w\-]+)*\/?(\?([\w\-\.,@?^=%&:\/~\+#]*)+)?$";

        /// <summary>
        /// 英文字母验证正则表达式
        /// </summary>
        public const string LetterPattern = @"^[a-zA-Z]+$";

        /// <summary>
        /// 车牌号验证正则表达式
        /// </summary>
        public const string PlateNumberOfChinaPatter =
            @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}[A-Z0-9]{4}[A-Z0-9挂学警港澳]{1}$";

        /// <summary>
        /// 邮政编码验证正则表达式
        /// </summary>
        public const string PostalCodeOfChinaPatter = @"^\d{6}$";

        /// <summary>
        /// QQ验证正则表达式
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const string QQPatter = @"^[1-9][0-9]{4,10}$";

        /// <summary>
        /// 微信号验证正则表达式
        /// </summary>
        public const string WechatNoPatter = @"^[a-zA-Z]([-_a-zA-Z0-9]{5,19})+$";

        /// <summary>
        /// 固话验证正则表达式
        /// </summary>
        public const string TelNoOfChinaPatter = @"^\d{3,4}-?\d{6,8}$";

        /// <summary>
        /// 社会信用代码正则表达式
        /// <para>法人和其他组织统一社会信用代码制度</para>
        /// </summary>
        public const string CreditCodePatter = @"^[0-9A-HJ-NPQRTUWXY]{2}\d{6}[0-9A-HJ-NPQRTUWXY]{10}$";

        /// <summary>
        /// 时间验证正则表达式
        /// </summary>
        public const string TimePatter = @"^([01]?[0-9]|2[0-3]):[0-5][0-9]$";

        /// <summary>
        /// 日期验证正则表达式
        /// </summary>
        public const string DatePatter = @"^((((0?[1-9]|[12]\d|3[01])[\.\-\/](0?[13578]|1[02])[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}))|((0?[1-9]|[12]\d|30)[\.\-\/](0?[13456789]|1[012])[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}))|((0?[1-9]|1\d|2[0-8])[\.\-\/]0?2[\.\-\/]((1[6-9]|[2-9]\d)?\d{2}))|(29[\.\-\/]0?2[\.\-\/]((1[6-9]|[2-9]\d)?(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)|00)))|(((0[1-9]|[12]\d|3[01])(0[13578]|1[02])((1[6-9]|[2-9]\d)?\d{2}))|((0[1-9]|[12]\d|30)(0[13456789]|1[012])((1[6-9]|[2-9]\d)?\d{2}))|((0[1-9]|1\d|2[0-8])02((1[6-9]|[2-9]\d)?\d{2}))|(2902((1[6-9]|[2-9]\d)?(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)|00)))) ?((20|21|22|23|[01]\d|\d)(([:.][0-5]\d){1,2}))?$";

        /// <summary>
        /// 浮点类型验证正则表达式
        /// </summary>
        public const string DecimalPatter = @"^((-?[1-9]+)|[0-9]+)(\.?|\,?)([0-9]*)$";

        /// <summary>
        /// 登录账号验证正则表达式
        /// </summary>
        public const string LoginPatter = "^[a-z0-9_-]{10,50}$";
    }
}
