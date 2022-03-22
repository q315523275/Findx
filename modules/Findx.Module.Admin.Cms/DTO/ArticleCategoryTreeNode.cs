using Findx.Data;
using System.Collections;
using System.Collections.Generic;

namespace Findx.Module.Admin.Cms.DTO
{
    /// <summary>
    /// 组织机构节点
    /// </summary>
    public class ArticleCategoryTreeNode : ITreeNode<long>
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
        public List<ArticleCategoryTreeNode> Children { get; set; } = new List<ArticleCategoryTreeNode>();

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
            Children = (List<ArticleCategoryTreeNode>)children;
        }
    }
}
