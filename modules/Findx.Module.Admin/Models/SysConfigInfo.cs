using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统参数配置表
    /// </summary>
    [Table(Name = "sys_config")]
    public class SysConfigInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
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
        /// 常量所属分类的编码，来自于“常量的分类”字典
        /// </summary>
        [Column(Name = "group_code")]
        public string GroupCode { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(Name = "name", DbType = "varchar(100)")]
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "remark")]
        public string Remark { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public sbyte Status { get; set; }

        /// <summary>
        /// 是否是系统参数（Y-是，N-否）
        /// </summary>
        [Column(Name = "sys_flag", DbType = "char(1)")]
        public string SysFlag { get; set; }

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
        [Column(Name = "value")]
        public string Value { get; set; }
    }
}
