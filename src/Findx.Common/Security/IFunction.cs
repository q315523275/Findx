using System;
using Findx.Data;

namespace Findx.Security
{
	/// <summary>
	/// 定义功能信息
	/// </summary>
	public interface IFunction: IEntity<Guid>
	{
        /// <summary>
        /// 获取或设置 功能名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 获取或设置 区域名称
        /// </summary>
        string Area { get; set; }

        /// <summary>
        /// 获取或设置 控制器名称
        /// </summary>
        string Controller { get; set; }

        /// <summary>
        /// 获取或设置 控制器的功能名称
        /// </summary>
        string Action { get; set; }

        /// <summary>
        /// 获取或设置 是否是控制器
        /// </summary>
        bool IsController { get; set; }

        /// <summary>
        /// 获取或设置 访问类型
        /// </summary>
        FunctionAccessType AccessType { get; set; }

        /// <summary>
        /// 获取或设置 是否启用操作审计
        /// </summary>
        bool AuditOperationEnabled { get; set; }
        
        /// <summary>
        /// 获取或设置 限定角色
        /// </summary>
        string Roles { get; set; }
        
        /// <summary>
        /// 获取或设置 是否锁定
        /// </summary>
        public bool IsLocked { get; set; }
    }
}

