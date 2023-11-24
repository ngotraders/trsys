using MediatR;
using System;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetUser : IRequest<UserDto>
    {
        public GetUser(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
