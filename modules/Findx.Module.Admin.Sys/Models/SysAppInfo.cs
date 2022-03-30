using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统应用表
    /// </summary>
    [Table(Name = "sys_app")]
    public class SysAppInfo : EntityBase<long>, IFullAudited<long>, ISort, IResponse, IRequest
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 是否默认激活（Y-是，N-否）
        /// </summary>
        [Column(Name = "active", DbType = "varchar(1)")]
        public string Active { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [Column(Name = "code", DbType = "varchar(50)")]
        public string Code { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        [Column(Name = "name", DbType = "varchar(100)")]
        public string Name { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public int Status { get; set; }

        /// <summary>
		/// 排序
		/// </summary>
		[Column(Name = "sort")]
		public int Sort { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Name = "create_user")]
        public long? CreatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "datetime")]
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Column(Name = "update_user")]
        public long? LastUpdaterId { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [Column(Name = "update_time", DbType = "datetime")]
        public DateTime? LastUpdatedTime { get; set; }
    }
}
