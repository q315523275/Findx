using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限处理器
    /// </summary>
    public interface IPermissionHandler
    {
        /// <summary>
        /// 初始化权限资源
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// 刷新功能信息缓存
        /// </summary>
        void RefreshCache();

        /// <summary>
        /// 清空功能信息缓存
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 获取权限信息
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Permission GetPermission(string area, string controller, string action);

        /// <summary>
        /// 获取权限访问信息
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        PermissionAccess GetPermissionAccess(string area, string controller, string action);
    }
}
