using System.ComponentModel;

namespace Findx.Module.Admin.Enum
{
    internal enum AdminTypeEnum
    {
        /// <summary>
        /// 超级管理员
        /// </summary>
        [Description("超级管理员")]
        SuperAdmin = 1,

        /// <summary>
        /// 普通账号
        /// </summary>
        [Description("普通账号")]
        None = 2
    }
}
