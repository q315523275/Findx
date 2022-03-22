namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 通知公告参数
    /// </summary>
    public class SysNoticeQuery : Findx.Data.PageBase
    {
        /// <summary>
        /// 类型（字典 1通知 2公告）
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string SearchValue { get; set; }
    }
}
