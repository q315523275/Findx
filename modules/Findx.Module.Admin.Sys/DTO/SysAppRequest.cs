using Findx.Data;

namespace Findx.Module.Admin.Sys.DTO
{
    public class SysAppRequest : IRequest
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 是否默认激活（Y-是，N-否）
        /// </summary>
        public string Active { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }
}
