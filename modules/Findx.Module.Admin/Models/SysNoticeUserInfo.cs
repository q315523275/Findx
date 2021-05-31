using System;

namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 系统用户数据范围表
    /// </summary>
    public partial class SysNoticeUser
    {

        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 通知公告id
        /// </summary>
        public long NoticeId { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        public DateTime? ReadTime { get; set; }

        /// <summary>
        /// 状态（字典 0未读 1已读）
        /// </summary>
        public sbyte Status { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }

    }

}
