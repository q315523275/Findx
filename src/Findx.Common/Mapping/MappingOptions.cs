using Microsoft.Extensions.Options;

namespace Findx.Mapping
{
    /// <summary>
    /// 实体映射配置
    /// </summary>
    public class MappingOptions : IOptions<MappingOptions>
    {
        /// <summary>
        /// this
        /// </summary>
        public MappingOptions Value => this;

        /// <summary>
        /// 是否忽略大小写
        /// </summary>
        public bool IgnoreCase { set; get; } = true;

        /// <summary>
        /// 是否空值
        /// </summary>
        public bool IgnoreNullValues { set; get; } = false;
    }
}
