using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    public class SysAppQuery : PageBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }
    }
}
