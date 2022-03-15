using System;
using System.ComponentModel.DataAnnotations;
using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.Cms.Models
{
    /// <summary>
    /// 信息分类表
    /// </summary>
    [Table(Name = "cms_article_category")]
    public class ArticleCategoryInfo : EntityBase<long>, ICreateUser<long>, IUpdateUser<long>, IResponse, IRequest, ISort
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
        /// 所属父类
        /// </summary>
        [Column(Name = "parent_id")]
        public long ParentId { get; set; } = 0;

        /// <summary>
        /// 调用别名
        /// </summary>
        [Column(Name = "call_index")]
        public string CallIndex { get; set; }

        /// <summary>
        /// 类别深度
        /// </summary>
        [Column(Name = "class_layer")]
        public int ClassLayer { get; set; } = 1;

        /// <summary>
        /// 标题
        /// </summary>
        [Column(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [Column(Name = "img_url")]
        public string ImgUrl { get; set; }

        /// <summary>
        /// 外部链接
        /// </summary>
        [Column(Name = "link_url")]
        public string LinkUrl { get; set; }

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
        /// 状态（字典 0正常 1停用 2删除）
        /// </summary>
        [Column(Name = "status", DbType = "tinyint(4)")]
        public int Status { get; set; }

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

