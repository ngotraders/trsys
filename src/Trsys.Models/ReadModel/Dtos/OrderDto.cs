﻿using System;

namespace Trsys.Models.ReadModel.Dtos
{
    public class OrderDto
    {
        public string Id { get; set; }
        public Guid SecretKeyId { get; set; }
        public int TicketNo { get; set; }
        public PublishedOrder Order { get; set; }
    }
}
