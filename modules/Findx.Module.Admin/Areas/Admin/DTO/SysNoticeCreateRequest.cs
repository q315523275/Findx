using Findx.Data;
using Findx.Module.Admin.Internals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    /// <summary>
    /// 
    /// </summary>
    public class SysNoticeCreateRequest: IRequest
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空")]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "内容不能为空")]
        public string Content { get; set; }

        /// <summary>
        /// 类型（字典 1通知 2公告）
        /// </summary>
        [Required(ErrorMessage = "类型不能为空")]
        public int Type { get; set; }

        /// <summary>
        /// 状态（字典 0草稿 1发布 2撤回 3删除）
        /// </summary>
        [Required(ErrorMessage = "状态不能为空")]
        public int Status { get; set; }

        /// <summary>
        /// 通知到的人
        /// </summary>
        [Required(ErrorMessage = "通知到的人不能为空")]
        public List<long> NoticeUserIdList { get; set; }
    }
}
