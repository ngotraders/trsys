using CQRSlite.Commands;
using CQRSlite.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class SecretKeyCommandHandlers : ICancellableCommandHandler<CreateSecretKeyCommand>
    {
        private readonly ISession session;

        public SecretKeyCommandHandlers(ISession session)
        {
            this.session = session;
        }

        public async Task Handle(CreateSecretKeyCommand message, CancellationToken token = default)
        {
            var item = new SecretKey(Guid.NewGuid(), message.Key);
            await session.Add(item);
            await session.Commit();
        }
    }
}
