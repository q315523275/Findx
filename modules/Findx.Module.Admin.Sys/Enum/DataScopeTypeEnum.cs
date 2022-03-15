namespace Findx.Module.Admin.Enum
{
    internal enum DataScopeTypeEnum
    {
        /// <summary>
        /// 全部数据
        /// </summary>
        ALL = 1,
        /// <summary>
        /// 本部门及以下数据
        /// </summary>
        DEPT_WITH_CHILD = 2,
        /// <summary>
        /// 本部门数据
        /// </summary>
        DEPT = 3,
        /// <summary>
        /// 仅本人数据
        /// </summary>
        SELF = 4,
        /// <summary>
        /// 自定义数据
        /// </summary>
        DEFINE = 5,
    }
}
