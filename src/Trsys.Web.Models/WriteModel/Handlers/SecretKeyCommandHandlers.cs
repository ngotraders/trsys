using CQRSlite.Domain;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;
using Trsys.Web.Models.WriteModel.Extensions;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class SecretKeyCommandHandlers : IRequestHandler<CreateSecretKeyCommand, Guid>
    {
        private readonly ISession session;

        public SecretKeyCommandHandlers(ISession session)
        {
            this.session = session;
        }

        public async Task<Guid> Handle(CreateSecretKeyCommand request, CancellationToken cancellationToken = default)
        {
            var state = await session.GetWorldState();
            var key = request.Key ?? Guid.NewGuid().ToString();
            if (state.GenerateSecretKeyIdIfNotExists(key, out var secretKeyId))
            {
                var item = new SecretKeyAggregate(secretKeyId, key);
                if (request.KeyType.HasValue)
                {
                    item.ChangeKeyType(request.KeyType.Value);
                }
                if (!string.IsNullOrEmpty(request.Description))
                {
                    item.ChangeDescription(request.Description);
                }
                await session.Add(state, cancellationToken);
                await session.Add(item, cancellationToken);
                await session.Commit(cancellationToken);
                return secretKeyId;
            }
            return secretKeyId;
        }
    }
}
