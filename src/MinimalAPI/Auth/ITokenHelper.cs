using System.Security.Claims;

namespace MinimalAPI.Auth
{
    public interface ITokenHelper
    {
        public Token GetAccessToken(IEnumerable<Claim> claims);
    }
}
