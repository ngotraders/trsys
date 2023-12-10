using MediatR;
using System;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetUserPasswordHash : IRequest<UserPasswordHashDto>
    {
        public GetUserPasswordHash(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
