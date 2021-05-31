namespace Findx.Security.Authorization
{
    public enum PermissionStoreType
    {
        /// <summary>
        /// 内存存储
        /// </summary>
        Memory = 0,
        /// <summary>
        /// 数据库存储
        /// </summary>
        DataBase = 1,
        /// <summary>
        /// 远端存储
        /// </summary>
        Remote = 2,
    }
}
