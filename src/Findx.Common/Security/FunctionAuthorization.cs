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
    /// <param name="settingFactory"></param>
    public FunctionAuthorization(ISettingProviderFactory settingFactory) : base(settingFactory)
    {
        
    }
}