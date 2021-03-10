using Microsoft.AspNetCore.Authorization;

namespace Findx.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 单设备登录
        /// </summary>
        public bool SingleDeviceEnabled { set; get; }
    }
}
