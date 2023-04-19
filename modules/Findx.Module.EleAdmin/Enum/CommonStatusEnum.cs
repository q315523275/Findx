using System.ComponentModel;

namespace Findx.Module.EleAdmin.Enum
{
    /// <summary>
    /// 通用状态 枚举
    /// </summary>
    public enum CommonStatusEnum
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")] ENABLE = 0,

        /// <summary>
        /// 停用
        /// </summary>
        [Description("停用")] DISABLE = 1,
    }
}