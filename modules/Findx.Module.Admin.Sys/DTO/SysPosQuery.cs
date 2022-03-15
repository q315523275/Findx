using Findx.Data;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 职位查询入参
    /// </summary>
    public class SysPosQuery : PageBase
    {
        /// <summary>
        /// 职位名称
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { set; get; }
    }
}
