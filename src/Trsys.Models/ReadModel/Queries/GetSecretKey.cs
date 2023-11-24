using MediatR;
using System;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetSecretKey : IRequest<SecretKeyDto>
    {
        public GetSecretKey(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
