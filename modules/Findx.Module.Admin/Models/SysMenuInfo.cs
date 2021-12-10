using Findx.Data;
using FreeSql.DataAnnotations;
using System;

namespace Findx.Module.Admin.Models
{
    /// <summary>
    /// 系统菜单表
    /// </summary>
    [Table(Name = "sys_menu")]
    public class SysMenuInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 应用分类（应用编码）
        /// </summary>
        [Column(Name = "application", DbType = "varchar(50)")]
        public string Application { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [Column(Name = "code", DbType = "varchar(50)")]
        public string Code { get; set; }

        /// <summary>
        /// 组件地址
        /// </summary>
        [Column(Name = "component")]
        public string Component { get; set; }

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
        /// 图标
        /// </summary>
        [Column(Name = "icon")]
        public string Icon { get; set; }

        /// <summary>
        /// 链接地址
        /// </summary>
        [Column(Name = "link")]
        public string Link { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column(Name = "name", DbType = "varchar(100)")]
        public string Name { get; set; }

        /// <summary>
        /// 打开方式（字典 0无 1组件 2内链 3外链）
        /// </summary>
        [Column(Name = "open_type", DbType = "tinyint(4)")]
        public int OpenType { get; set; }

        /// <summary>
        /// 权限标识
        /// </summary>
        [Column(Name = "permission")]
        public string Permission { get; set; }

        /// <summary>
        /// 父id
        /// </summary>
        [Column(Name = "pid")]
        public long Pid { get; set; }

        /// <summary>
        /// 父ids
        /// </summary>
        [Column(Name = "pids", DbType = "text")]
        public string Pids { get; set; }

        /// <summary>
        /// 重定向地址
        /// </summary>
        [Column(Name = "redirect")]
        public string Redirect { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column(Name = "remark")]
        public string Remark { get; set; }

        /// <summary>
        /// 路由地址
        /// </summary>
        [Column(Name = "router")]
        public string Router { get; set; }

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
        /// 菜单类型（字典 0目录 1菜单 2按钮）
        /// </summary>
        [Column(Name = "type", DbType = "tinyint(4)")]
        public int Type { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column(Name = "update_time", DbType = "datetime")]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        [Column(Name = "update_user")]
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 是否可见（Y-是，N-否）
        /// </summary>
        [Column(Name = "visible", DbType = "char(1)")]
        public string Visible { get; set; }

        /// <summary>
        /// 权重（字典 1系统权重 2业务权重）
        /// </summary>
        [Column(Name = "weight", DbType = "tinyint(4)")]
        public int? Weight { get; set; }

    }
}
