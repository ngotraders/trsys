using System;

namespace Trsys.Web.Infrastructure.Tokens
{
    public class TokenConnectionEventArgs : EventArgs
    {
        public TokenConnectionEventArgs(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}