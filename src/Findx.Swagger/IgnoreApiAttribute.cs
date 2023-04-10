using System;

namespace Findx.Swagger
{
    /// <summary>
    /// 忽略API属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IgnoreApiAttribute : Attribute {}
}
