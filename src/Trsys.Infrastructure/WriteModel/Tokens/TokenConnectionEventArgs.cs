﻿using System;

namespace Trsys.Infrastructure.WriteModel.Tokens
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