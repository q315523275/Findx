using System;
using System.Collections;
using System.Collections.Generic;
using Findx.Data;

namespace Findx.Module.Admin.Cms.DTO
{
	public class ArticleCategoryOutput : ITreeNode<long>, IResponse
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 所属频道
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
		/// 父id
		/// </summary>
        public long Pid { get; set; }

        /// <summary>
        /// 父ids
        /// </summary>
        public string Pids { get; set; }

        /// <summary>
        /// 调用别名
        /// </summary>
        public string CallIndex { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 外部链接
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 介绍
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }


        public long GetId()
        {
            return Id;
        }

        public long GetPid()
        {
            return Pid;
        }

        public void SetChildren(IList children)
        {
            Children = (List<ArticleCategoryOutput>)children;
        }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<ArticleCategoryOutput> Children { get; set; } = new List<ArticleCategoryOutput>();
    }
}

