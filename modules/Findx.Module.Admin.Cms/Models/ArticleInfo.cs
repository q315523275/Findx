using System;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Cms.Models
{
	/// <summary>
	/// 信息表
	/// </summary>
	[Table(Name = "cms_article")]
	public class ArticleInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest, ISort
	{
        /// <summary>
        /// 编号
        /// </summary>
        [Column(Name = "id", IsPrimary = true)]
        public override long Id { get; set; }

        /// <summary>
        /// 所属频道
        /// </summary>
        [Column(Name = "channel_id")]
        public int ChannelId { get; set; }

        /// <summary>
        /// 类别ID
        /// </summary>
        [Column(Name = "category_id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Column(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        [Column(Name = "source")]
        public string Source { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Column(Name = "author")]
        public string Author { get; set; }

        /// <summary>
        /// 外部链接
        /// </summary>
        [Column(Name = "link_url")]
        public string LinkUrl { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [Column(Name = "img_url")]
        public string ImgUrl { get; set; }

        /// <summary>
        /// SEO标题
        /// </summary>
        [Column(Name = "seo_title")]
        public string SeoTitle { get; set; }

        /// <summary>
        /// SEO关健字
        /// </summary>
        [Column(Name = "seo_keyword")]
        public string SeoKeyword { get; set; }

        /// <summary>
        /// SEO描述
        /// </summary>
        [Column(Name = "seo_desc")]
        public string SeoDescription { get; set; }

        /// <summary>
        /// 内容摘要
        /// </summary>
        [Column(Name = "zhaiyao")]
        public string Zhaiyao { get; set; }

        /// <summary>
        /// 内容介绍
        /// </summary>
        [Column(Name = "content")]
        public string Content { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column(Name = "sort")]
        public int Sort { get; set; }

        /// <summary>
        /// 浏览总数
        /// </summary>
        [Column(Name = "click")]
        public int Click { get; set; } = 0;

        /// <summary>
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public int Status { get; set; }

        /// <summary>
        /// 评论0禁止1允许
        /// </summary>
        [Column(Name = "is_comment", DbType = "tinyint(4)")]
        public byte IsComment { get; set; } = 0;

        /// <summary>
        /// 评论总数
        /// </summary>
        [Column(Name = "comment_count")]
        public int CommentCount { get; set; } = 0;

        /// <summary>
        /// 点赞总数
        /// </summary>
        [Column(Name = "like_count")]
        public int LikeCount { get; set; } = 0;

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

