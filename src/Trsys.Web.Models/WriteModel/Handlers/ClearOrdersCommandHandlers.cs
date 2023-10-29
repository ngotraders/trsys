using CQRSlite.Domain;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class ClearOrdersCommandHandlers :
        IRequestHandler<ClearOrdersCommand>
    {
        private readonly IRepository repository;

        public ClearOrdersCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(ClearOrdersCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.Publish(new List<PublishedOrder>());
            await repository.Save(publisher, null, cancellationToken);
        }
    }
}
