using System;

namespace Findx.RPC.Attributes
{
    /// <summary>
    /// Rpc标记属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class RpcAttribute: Attribute
    {
        
    }
}