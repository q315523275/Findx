using Microsoft.Extensions.Options;

namespace Findx.Caching
{
    public class CachingOptions : IOptions<CachingOptions>
    {
        public CachingOptions Value => this;
        /// <summary>
        /// 默认缓存
        /// </summary>
        public string Primary { set; get; }

        public override string ToString()
        {
            return Primary;
        }
    }
}
