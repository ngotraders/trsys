using System.Threading.Tasks;
using Trsys.Web.Models;

namespace Trsys.Web.Services
{
    public class TokenService : ITokenGenerator, ITokenValidator
    {
        public Task<TokenGenerationResult> GenerateTokenAsync(string secretKey)
        {
            return Task.FromResult(TokenGenerationResult.Success("VALID_TOKEN"));
        }

        public bool Validate(string token)
        {
            return token == "VALID_TOKEN";
        }
    }
}
