
using System;
using Findx.Data;

namespace Findx.Module.Admin.Cms.DTO
{
    /// <summary>
    /// 信息类别入参
    /// </summary>
	public class ArticleCategoryRequest: IRequest
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
        /// 所属父类
        /// </summary>
        public long ParentId { get; set; } = 0;

        /// <summary>
        /// 调用别名
        /// </summary>
        public string CallIndex { get; set; }

        /// <summary>
        /// 类别深度
        /// </summary>
        public int ClassLayer { get; set; } = 1;

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
        /// 内容介绍
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// SEO标题
        /// </summary>
        public string SeoTitle { get; set; }

        /// <summary>
        /// SEO关健字
        /// </summary>
        public string SeoKeyword { get; set; }

        /// <summary>
        /// SEO描述
        /// </summary>
        public string SeoDescription { get; set; }

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        public int Status { get; set; }
    }
}

