using Findx.Data;
using System;
using System.Collections.Generic;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 消息公告出参
    /// </summary>
    public class SysNoticeOutput : IResponse
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
        /// 发布机构id
        /// </summary>
        public long? PublicOrgId { get; set; }

        /// <summary>
        /// 发布机构名称
        /// </summary>
        public string PublicOrgName { get; set; }

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
        public string PublicUserName { get; set; }

        /// <summary>
        /// 状态（字典 0草稿 1发布 2撤回 3删除）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 类型（字典 1通知 2公告）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 通知到的用户Id集合
        /// </summary>
        public List<string> NoticeUserIdList { get; set; }

        /// <summary>
        /// 通知到的用户阅读信息集合
        /// </summary>
        public List<NoticeUserRead> NoticeUserReadInfoList { get; set; }
    }

    public class NoticeUserRead
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 状态（字典 0未读 1已读）
        /// </summary>
        public int ReadStatus { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        public DateTimeOffset? ReadTime { get; set; }
    }
}
