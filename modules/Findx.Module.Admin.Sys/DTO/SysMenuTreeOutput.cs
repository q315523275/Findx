using System;
using System.Collections.Generic;
using Findx.Data;
using Findx.Module.Admin.Enum;

namespace Findx.Module.Admin.Sys.DTO
{
	/// <summary>
	/// 菜单树---授权、新增编辑时选择
	/// </summary>
	public class SysMenuTreeOutput : ITreeNode<long>
	{
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 父Id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value => IntValue.ToString();

        public long IntValue { get; set; }

        /// <summary>
        /// 排序，越小优先级越高
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<SysMenuTreeOutput> Children { get; set; } = new List<SysMenuTreeOutput>();

        public long GetId()
        {
            return Id;
        }

        public long GetPid()
        {
            return ParentId;
        }

        public void SetChildren(System.Collections.IList children)
        {
            Children = (List<SysMenuTreeOutput>)children;
        }
    }
}

