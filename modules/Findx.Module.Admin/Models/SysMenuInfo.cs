using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统菜单表
    /// </summary>
    public partial class SysMenu
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 应用分类（应用编码）
        /// </summary>
        public string Application { get; set; } = string.Empty;

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 组件地址
        /// </summary>
        public string Component { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 链接地址
        /// </summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 打开方式（字典 0无 1组件 2内链 3外链）
        /// </summary>
        public sbyte OpenType { get; set; }

        /// <summary>
        /// 权限标识
        /// </summary>
        public string Permission { get; set; } = string.Empty;

        /// <summary>
        /// 父id
        /// </summary>
        public long Pid { get; set; }

        /// <summary>
        /// 父ids
        /// </summary>
        public string Pids { get; set; } = string.Empty;

        /// <summary>
        /// 重定向地址
        /// </summary>
        public string Redirect { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;

        /// <summary>
        /// 路由地址
        /// </summary>
        public string Router { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 菜单类型（字典 0目录 1菜单 2按钮）
        /// </summary>
        public sbyte Type { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? UpdateUser { get; set; }

        /// <summary>
        /// 是否可见（Y-是，N-否）
        /// </summary>
        public string Visible { get; set; } = string.Empty;

        /// <summary>
        /// 权重（字典 1系统权重 2业务权重）
        /// </summary>
        public sbyte? Weight { get; set; }

    }

}
