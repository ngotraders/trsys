using MediatR;
using System.Collections.Generic;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.Models.ReadModel.Queries
{
    public class GetSecretKeys : IRequest<List<SecretKeyDto>>
    {
        public GetSecretKeys()
        {
        }
    }
}
