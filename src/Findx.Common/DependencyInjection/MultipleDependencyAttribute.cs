using System;

namespace Findx.DependencyInjection
{
    /// <summary>
    /// 标注了此特性的类，将允许接口进行多次注入
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class MultipleDependencyAttribute : Attribute
    { }
}
