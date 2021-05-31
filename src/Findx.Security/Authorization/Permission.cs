using Findx.Extensions;
using System.ComponentModel;
namespace Findx.Security.Authorization
{
    /// <summary>
    /// 权限资源信息
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// 私有编号
        /// </summary>
        private string _code;

        /// <summary>
        /// 编号
        /// </summary>
        public string Code
        {
            get
            {
                if (_code.IsNullOrWhiteSpace())
                {
                    _code = $"{Area}-{Controller}-{Action}".ToMd5();
                }
                return _code;
            }
        }

        /// <summary>
        /// 功能名称
        /// </summary>
        [DisplayName("名称")]
        public string Name { get; set; }

        /// <summary>
        /// 区域名称
        /// </summary>
        [DisplayName("区域")]
        public string Area { get; set; }

        /// <summary>
        /// 控制器名称
        /// </summary>
        [DisplayName("控制器")]
        public string Controller { get; set; }

        /// <summary>
        /// 控制器的功能名称
        /// </summary>
        [DisplayName("功能")]
        public string Action { get; set; }

        /// <summary>
        /// 是否是控制器
        /// </summary>
        [DisplayName("是否控制器")]
        public bool IsController { get; set; }

        /// <summary>
        /// 访问类型
        /// </summary>
        [DisplayName("访问类型")]
        public PermiessionAccessType AccessType { get; set; }

        /// <summary>
        /// 是否验证角色
        /// </summary>
        [DisplayName("验证角色")]
        public string Roles { get; set; }

        /// <summary>
        /// 是否启用操作审计
        /// </summary>
        [DisplayName("是否操作审计")]
        public bool AuditEnabled { get; set; }
    }
}
