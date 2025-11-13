using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     令牌验证器接口
/// </summary>
public interface IAdvancedTokenValidator
{
    /// <summary>
    ///     验证生命期
    /// </summary>
    /// <param name="notBefore"></param>
    /// <param name="expires"></param>
    /// <param name="securityToken"></param>
    /// <param name="validationParameters"></param>
    /// <returns></returns>
    bool ValidateLifetime(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters);
}