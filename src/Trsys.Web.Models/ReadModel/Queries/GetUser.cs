using MediatR;
using System;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
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
