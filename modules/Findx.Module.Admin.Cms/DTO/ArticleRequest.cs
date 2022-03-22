
using System;
using Findx.Data;

namespace Findx.Module.Admin.Cms.DTO
{
    /// <summary>
    /// 信息类别入参
    /// </summary>
	public class ArticleRequest : IRequest
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
        /// 类别ID
        /// </summary>
        public long CategoryId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 外部链接
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 内容摘要
        /// </summary>
        public string Zhaiyao { get; set; }

        /// <summary>
        /// 内容介绍
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 浏览总数
        /// </summary>
        public int Click { get; set; } = 0;

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public int Status { get; set; }
    }
}

