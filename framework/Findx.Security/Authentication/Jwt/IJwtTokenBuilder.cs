using System.Collections.Generic;
using System.Threading.Tasks;

namespace Findx.Security.Authentication.Jwt;

/// <summary>
///     JwtToken构建器
/// </summary>
public interface IJwtTokenBuilder
{
    /// <summary>
    ///     创建JwtToken
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="jwtOption"></param>
    /// <returns></returns>
    Task<JwtToken> CreateAsync(IDictionary<string, string> payload, JwtOptions jwtOption);
}