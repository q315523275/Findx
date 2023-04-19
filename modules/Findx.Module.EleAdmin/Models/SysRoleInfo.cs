using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.EleAdmin.Models
{
    /// <summary>
    /// 角色
    /// </summary>
    [Table(Name = "sys_role")]
    [EntityExtension(DataSource = "system")]
    public class SysRoleInfo : FullAuditedBase<Guid, Guid>, ISoftDeletable, ITenant, IResponse
    {
        /// <summary>
        /// 角色id
        /// </summary>
        [Column(IsPrimary = true, IsIdentity = true)]
        public override Guid Id { get; set; }

        /// <summary>
        /// 应用编号
        /// </summary>
        public string ApplicationCode { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色标识
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeletionTime { get; set; }
    }
}