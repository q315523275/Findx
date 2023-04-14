using Findx.Setting;

namespace Findx.Security;

/// <summary>
/// 功能权限验证类
/// </summary>
public class FunctionAuthorization : FunctionAuthorizationBase
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="settingProvider"></param>
    public FunctionAuthorization(ISettingProvider settingProvider) : base(settingProvider)
    {
        
    }
}