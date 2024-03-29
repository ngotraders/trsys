﻿using MediatR;
using System;

namespace Trsys.Models.WriteModel.Commands
{
    public class SecretKeyCreateCommand : IRequest<Guid>, IRetryableRequest
    {
        public SecretKeyCreateCommand(SecretKeyType? keyType, string key, string description, bool? approve = null)
        {
            KeyType = keyType;
            Key = key;
            Description = description;
            Approve = approve;
        }

        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool? Approve { get; set; }
    }
}
