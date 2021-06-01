using MediatR;
using System;

namespace Trsys.Web.Models.WriteModel.Commands
{
    public class FetchOrderCommand : IRequest
    {
        public FetchOrderCommand(Guid id, int[] tickets)
        {
            Id = id;
            Tickets = tickets;
        }

        public Guid Id { get; }
        public int[] Tickets { get; }
    }
}
