using CQRSlite.Messages;
using CQRSlite.Queries;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.ReadModel.Dtos;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.ReadModel.Queries;

namespace Trsys.Web.Models.ReadModel.Handlers
{
    public class SecretKeyListView : ICancellableHandler<SecretKeyCreated>, ICancellableQueryHandler<GetSecretKeys, List<SecretKeyDto>>
    {
        public Task Handle(SecretKeyCreated message, CancellationToken token = default)
        {
            InMemoryDatabase.List.Add(new SecretKeyDto()
            {
                Id = message.Id,
                Key = message.Key
            });
            return Task.CompletedTask;
        }

        public Task<List<SecretKeyDto>> Handle(GetSecretKeys message, CancellationToken token = default)
        {
            return Task.FromResult(InMemoryDatabase.List);
        }
    }
}
