using System;
using Microsoft.IdentityModel.Tokens;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     默认令牌验证器
/// </summary>
public class DefaultAdvancedTokenValidator: IAdvancedTokenValidator
{
    /// <summary>
    ///     验证生命周期
    /// </summary>
    /// <param name="notBefore"></param>
    /// <param name="expires"></param>
    /// <param name="securityToken"></param>
    /// <param name="validationParameters"></param>
    /// <returns></returns>
    public bool ValidateLifetime(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
    {
        return true;
    }
}