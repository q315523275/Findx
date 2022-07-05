using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Security
{
    /// <summary>
    /// 功能信息基类
    /// </summary>
    public abstract class FunctionBase : EntityBase<Guid>, IFunction
    {
        /// <summary>
        /// 获取或设置 功能名称
        /// </summary>
        [DisplayName("名称"), StringLength(200)]
        public virtual string Name { get; set; }

        /// <summary>
        /// 获取或设置 区域名称
        /// </summary>
        [DisplayName("区域"), StringLength(200)]
        public virtual string Area { get; set; }

        /// <summary>
        /// 获取或设置 控制器名称
        /// </summary>
        [DisplayName("控制器"), StringLength(200)]
        public virtual string Controller { get; set; }

        /// <summary>
        /// 获取或设置 控制器的功能名称
        /// </summary>
        [DisplayName("功能"), StringLength(200)]
        public virtual string Action { get; set; }

        /// <summary>
        /// 获取或设置 是否是控制器
        /// </summary>
        [DisplayName("是否控制器")]
        public virtual bool IsController { get; set; }

        /// <summary>
        /// 获取或设置 访问类型
        /// </summary>
        [DisplayName("访问类型")]
        public virtual FunctionAccessType AccessType { get; set; }

        /// <summary>
        /// 获取或设置 限定角色
        /// </summary>
        [DisplayName("限定角色")]
        public virtual string Roles { get; set; }

        /// <summary>
        /// 获取或设置 是否启用操作审计
        /// </summary>
        [DisplayName("是否操作审计")]
        public virtual bool AuditOperationEnabled { get; set; }

        /// <summary>
        /// 获取或设置 是否锁定
        /// </summary>
        [DisplayName("是否锁定")]
        public virtual bool IsLocked { get; set; }

        /// <summary>
        /// 返回一个表示当前对象的 string。
        /// </summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString()
        {
            return $"{Name}[{Area}/{Controller}/{Action}]";
        }
    }
}
