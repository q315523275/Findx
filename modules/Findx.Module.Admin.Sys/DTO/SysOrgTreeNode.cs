﻿using Findx.Data;
using System.Collections;
using System.Collections.Generic;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 组织机构节点
    /// </summary>
    public class SysOrgTreeNode : ITreeNode<long>
    {
        /// <summary>
        /// Id
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
        public string Value { get; set; }

        /// <summary>
        /// 排序，越小优先级越高
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<SysOrgTreeNode> Children { get; set; } = new List<SysOrgTreeNode>();

        /// <summary>
        /// 上一级Id
        /// </summary>
        public long Pid { get; set; }

        public long GetId()
        {
            return Id;
        }

        public long GetPid()
        {
            return ParentId;
        }

        public void SetChildren(IList children)
        {
            Children = (List<SysOrgTreeNode>)children;
        }
    }
}