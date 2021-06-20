using MediatR;

namespace Trsys.Web.Models.WriteModel.Notifications
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
