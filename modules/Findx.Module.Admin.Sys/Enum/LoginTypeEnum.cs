using System.ComponentModel;

namespace Findx.Module.Admin.Enum
{
    /// <summary>
    /// 登陆类型
    /// </summary>
    public enum LoginTypeEnum
    {
        /// <summary>
        /// 登陆
        /// </summary>
        [Description("登陆")]
        LOGIN = 1,

        /// <summary>
        /// 登出
        /// </summary>
        [Description("登出")]
        LOGOUT = 2,

        /// <summary>
        /// 注册
        /// </summary>
        [Description("注册")]
        REGISTER = 3,

        /// <summary>
        /// 改密
        /// </summary>
        [Description("改密")]
        CHANGEPASSWORD = 4,

        /// <summary>
        /// 三方授权登陆
        /// </summary>
        [Description("授权登陆")]
        AUTHORIZEDLOGIN = 5
    }
}
