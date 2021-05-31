using MediatR;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenTouched : INotification
    {
        public TokenTouched(string token)
        {
            Token = token;
        }

        public string Token { get; }
    }
}
