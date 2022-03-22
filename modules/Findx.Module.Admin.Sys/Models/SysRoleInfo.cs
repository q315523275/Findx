﻿using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统角色表
    /// </summary>
    [Table(Name = "sys_role")]
    public class SysRoleInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, ISort, IResponse, IRequest
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [Column(Name = "code", DbType = "varchar(50)")]
        public string Code { get; set; }

        /// <summary>
        /// 数据范围类型（字典 1全部数据 2本部门及以下数据 3本部门数据 4仅本人数据 5自定义数据）
        /// </summary>
        [Column(Name = "data_scope_type", DbType = "tinyint(4)")]
        public int DataScopeType { get; set; }

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
        /// 序号
        /// </summary>
        [Column(Name = "sort")]
        public int Sort { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "create_time", DbType = "datetime")]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Column(Name = "create_user")]
        public long? CreateUser { get; set; }

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
        /// 初始化
        /// </summary>
        public override void Init()
        {
            Id = Findx.Utils.SnowflakeId.Default().NextId();
        }
    }
}
