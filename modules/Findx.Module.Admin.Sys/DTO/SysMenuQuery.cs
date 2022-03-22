using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
    /// <summary>
    /// 菜单查询入参
    /// </summary>
    public class SysMenuQuery : PageBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 应用分类（应用编码）
        /// </summary>
        public string Application { get; set; }
    }
}
