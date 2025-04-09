namespace Findx.RPC;

/// <summary>
///     Rpc服务路由属性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class RpcServiceRouteAttribute : Attribute
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="template"></param>
    public RpcServiceRouteAttribute(string template)
    {
        ArgumentNullException.ThrowIfNull(template, nameof (template));
        Template = template;
    }

    /// <summary>
    ///     路由模版
    /// </summary>
    public string Template { set; get; }
}