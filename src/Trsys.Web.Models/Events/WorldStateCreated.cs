﻿using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Web.Models.Events
{
    public class WorldStateCreated : INotification, IEvent
    {
        public WorldStateCreated(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
