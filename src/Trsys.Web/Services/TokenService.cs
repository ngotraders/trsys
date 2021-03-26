using Trsys.Web.Models;

namespace Trsys.Web.Services
{
    public class TokenService : ITokenValidator
    {
        public bool Validate(string token)
        {
            return true;
        }
    }
}
