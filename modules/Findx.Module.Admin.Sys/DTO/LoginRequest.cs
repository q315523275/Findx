namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 系统登录入参
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 账户
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string Code { get; set; }
    }
}
