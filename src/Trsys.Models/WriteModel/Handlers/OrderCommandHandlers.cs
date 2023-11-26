using CQRSlite.Domain;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Commands;
using Trsys.Models.WriteModel.Domain;

namespace Trsys.Models.WriteModel.Handlers
{
    public class OrderCommandHandlers :
        IRequestHandler<PublisherReplaceOrdersCommand>,
        IRequestHandler<PublisherClearOrdersCommand>,
        IRequestHandler<PublisherOpenOrderCommand>,
        IRequestHandler<PublisherCloseOrderCommand>
    {
        private readonly IRepository repository;

        public OrderCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(PublisherReplaceOrdersCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.ReplaceOrders(request.PublishedOrders);
            await repository.Save(publisher, null, cancellationToken);
        }

        public async Task Handle(PublisherClearOrdersCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.ReplaceOrders(new List<PublishedOrder>());
            await repository.Save(publisher, null, cancellationToken);
        }

        public async Task Handle(PublisherOpenOrderCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.OpenOrder(request.PublishedOrder);
            await repository.Save(publisher, null, cancellationToken);
        }

        public async Task Handle(PublisherCloseOrderCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.CloseOrder(request.TicketNo);
            await repository.Save(publisher, null, cancellationToken);
        }
    }
}
