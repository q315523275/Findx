using Findx.Data;

namespace Findx.Module.Admin.DTO
{
    /// <summary>
    /// 系统字典类型请求入参
    /// </summary>
    public class SysDictTypeQuery : PageBase
    {
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
