using CQRSlite.Events;
using MediatR;
using System;

namespace Trsys.Models.Events;

public class WorldStateConfigurationUpdated : INotification, IEvent
{
    public WorldStateConfigurationUpdated(Guid id, EmailConfiguration emailConfiguration)
    {
        Id = id;
        EmailConfiguration = emailConfiguration;
    }

    public Guid Id { get; set; }
    public int Version { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public EmailConfiguration EmailConfiguration { get; set; }
}
