using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Findx.Security.Authentication.Jwt
{
    public class DefaultJwtTokenBuilder : IJwtTokenBuilder
    {
        public Task<JwtToken> CreateAsync(IDictionary<string, string> payload)
        {
            var claims = payload.Keys.Select(key => new Claim(key, payload[key]?.ToString()));


            throw new NotImplementedException();
        }
    }
}
