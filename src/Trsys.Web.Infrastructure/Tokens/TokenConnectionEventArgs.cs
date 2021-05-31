using System;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionEventArgs : EventArgs
    {
        public TokenConnectionEventArgs(Guid id, string token)
        {
            Id = id;
            Token = token;
        }

        public Guid Id { get; }
        public string Token { get; }
    }
}