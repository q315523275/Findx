using Findx.Extensions;
namespace Findx.Caching
{
    /// <summary>
    /// 字符串缓存键生成器
    /// </summary>
    public class StringCacheKeyGenerator : ICacheKeyGenerator
    {
        /// <summary>
        /// 生成缓存键
        /// </summary>
        /// <param name="args">参数</param>
        public string GetKey(params object[] args)
        {
            Check.NotNull(args, nameof(args));

            return args.ExpandAndToString(":");
        }
    }
}
