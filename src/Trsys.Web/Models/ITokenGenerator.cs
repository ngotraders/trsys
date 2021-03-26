using System.Threading.Tasks;

namespace Trsys.Web.Models
{
    public interface ITokenGenerator
    {
        Task<TokenGenerationResult> GenerateTokenAsync(string secretKey);
    }

    public enum TokenGenerationFailureReason
    {
        InvalidToken,
        TokenInUse,
    }

    public class TokenGenerationResult
    {
        private TokenGenerationResult() { }

        public TokenGenerationFailureReason? Failure { get; private set; }
        public bool Succeeded { get; private set; }
        public string Token { get; private set; }

        public static TokenGenerationResult Success(string token) => new TokenGenerationResult() { Succeeded = true, Token = token };
        public static TokenGenerationResult Fail(TokenGenerationFailureReason reason) => new TokenGenerationResult() { Failure = reason };

    }
}
