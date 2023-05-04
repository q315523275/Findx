using Microsoft.AspNetCore.Authorization;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     预授权属性,继承AuthorizeAttribute
/// </summary>
public class PreAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    ///     权限资源
    /// </summary>
    public string Authority { set; get; }
}