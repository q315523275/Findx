using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 通知表
    /// </summary>
    public partial class SysNotice
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 撤回时间
        /// </summary>
        public DateTime? CancelTime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreateUser { get; set; }

        /// <summary>
        /// 发布机构id
        /// </summary>
        public long? PublicOrgId { get; set; }

        /// <summary>
        /// 发布机构名称
        /// </summary>
        public string PublicOrgName { get; set; } = string.Empty;

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublicTime { get; set; }

        /// <summary>
        /// 发布人id
        /// </summary>
        public long PublicUserId { get; set; }

        /// <summary>
        /// 发布人姓名
        /// </summary>
        public string PublicUserName { get; set; } = string.Empty;

        /// <summary>
        /// 状态（字典 0草稿 1发布 2撤回 3删除）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 类型（字典 1通知 2公告）
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

    }

}
