using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统字典值表
    /// </summary>
    [Table(Name = "sys_dict_data")]
    public partial class SysDictDataInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
    {

        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [Column(Name = "code", DbType = "varchar(50)")]
        public string Code { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "datetime")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Name = "create_user")]
        public long? CreateUser { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "remark")]
        public string Remark { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column(Name = "sort")]
        public int Sort { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public int Status { get; set; }

        /// <summary>
        /// 字典类型id
        /// </summary>
        [Column(Name = "type_id")]
        public long TypeId { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column(Name = "update_time", DbType = "datetime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Column(Name = "update_user")]
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [Column(Name = "value", DbType = "text")]
        public string Value { get; set; }

    }
}
