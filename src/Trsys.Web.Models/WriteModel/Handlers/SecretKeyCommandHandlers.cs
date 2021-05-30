using CQRSlite.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class SecretKeyCommandHandlers : IRequestHandler<CreateSecretKeyCommand>
    {
        private readonly ISession session;

        public SecretKeyCommandHandlers(ISession session)
        {
            this.session = session;
        }

        public async Task<Unit> Handle(CreateSecretKeyCommand request, CancellationToken cancellationToken = default)
        {
            var item = new SecretKeyAggregate(Guid.NewGuid(), request.Key);
            await session.Add(item);
            await session.Commit();
            return Unit.Value;
        }
    }
}
