using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimalAPI.Auth
{
    public class JwtTokenHelper : ITokenHelper
    {
        private readonly TokenOptions _tokenOptions;

        public JwtTokenHelper(IOptions<TokenOptions> tokenOptions)
        {
            _tokenOptions = tokenOptions.Value;
        }

        public Token GetAccessToken(IEnumerable<Claim> claims)
        {
            SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
            SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.Now.AddMinutes(_tokenOptions.Expiration);
            JwtSecurityToken jwtSecurityToken = new(
                _tokenOptions.Issuer,
                _tokenOptions.Audience,
                claims,
                expires: expiration,
                signingCredentials: signingCredentials,
                notBefore: DateTime.Now);

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            Token token = new(jwtSecurityTokenHandler.WriteToken(jwtSecurityToken), expiration);
            return token;
        }
    }
}
