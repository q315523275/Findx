namespace Findx.Aspect
{
    /// <summary>
    /// 代理工具
    /// </summary>
    public static class ProxyHelper
    {
        private const string ProxyNamespace = "Castle.Proxies";

        /// <summary>
        /// 取消代理
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object UnProxy(object obj)
        {
            if (obj.GetType().Namespace != ProxyNamespace)
            {
                return obj;
            }

            var targetField = obj.GetType()
                                 .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                 .FirstOrDefault(f => f.Name == "__target");

            if (targetField == null)
            {
                return obj;
            }

            return targetField.GetValue(obj);
        }

        /// <summary>
        /// 获取未代理类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Type GetUnProxiedType(object obj)
        {
            return UnProxy(obj).GetType();
        }
    }
}

