using CQRSlite.Domain;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Commands;
using Trsys.Models.WriteModel.Domain;

namespace Trsys.Models.WriteModel.Handlers
{
    public class FetchedOrderCommandHandlers :
        IRequestHandler<SubscriberFetchOrderCommand>
    {
        private readonly IRepository repository;

        public FetchedOrderCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(SubscriberFetchOrderCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.Subscribed(request.Tickets);
            await repository.Save(publisher, null, cancellationToken);
        }
    }
}
