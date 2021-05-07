using Castle.DynamicProxy;

namespace Findx.DynamicProxy
{
    internal static class CastleHelper
    {
        public static readonly IProxyBuilder ProxyBuilder = new DefaultProxyBuilder();

        public static readonly IProxyGenerator ProxyGenerator = new ProxyGenerator(ProxyBuilder);
    }
}
