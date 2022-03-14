using Findx.Data;

namespace Findx.Module.Admin.Areas.Admin.DTO
{
    public class SysOrgQuery : PageBase
    {
        /// <summary>
        /// 父及编号
        /// </summary>
        public string Pid { set; get; }

        /// <summary>
        /// 名词
        /// </summary>
        public string Name { set; get; }
    }
}
