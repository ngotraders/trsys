﻿using CQRSlite.Events;
using System;

namespace Trsys.Web.Models.ReadModel.Events
{
    public class SecretKeyTokenInvalidated : IEvent
    {
        public SecretKeyTokenInvalidated(Guid id, string token)
        {
            Id = id;
            Token = token;
        }

        public Guid Id { get; set; }
        public string Token { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}