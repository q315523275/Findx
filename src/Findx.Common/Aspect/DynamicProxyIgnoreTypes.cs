using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Extensions;
namespace Findx.Aspect
{
    public class DynamicProxyIgnoreTypes
    {
        private static HashSet<Type> IgnoredTypes { get; } = new HashSet<Type>();

        public static void Add<T>()
        {
            lock (IgnoredTypes)
            {
                IgnoredTypes.TryAdd(typeof(T));
            }
        }

        public static bool Contains(Type type, bool includeDerivedTypes = true)
        {
            lock (IgnoredTypes)
            {
                return includeDerivedTypes
                    ? IgnoredTypes.Any(t => t.IsAssignableFrom(type))
                    : IgnoredTypes.Contains(type);
            }
        }
    }
}

