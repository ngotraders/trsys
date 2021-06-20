using MediatR;
using System;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
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
