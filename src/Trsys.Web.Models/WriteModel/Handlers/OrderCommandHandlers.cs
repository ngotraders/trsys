using CQRSlite.Domain;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class OrderCommandHandlers :
        IRequestHandler<OrdersReplaceCommand>,
        IRequestHandler<OrdersClearCommand>,
        IRequestHandler<OrderOpenCommand>,
        IRequestHandler<OrderCloseCommand>
    {
        private readonly IRepository repository;

        public OrderCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(OrdersReplaceCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.ReplaceOrders(request.PublishedOrders);
            await repository.Save(publisher, null, cancellationToken);
        }

        public async Task Handle(OrdersClearCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.ReplaceOrders(new List<PublishedOrder>());
            await repository.Save(publisher, null, cancellationToken);
        }

        public async Task Handle(OrderOpenCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.OpenOrder(request.PublishedOrder);
            await repository.Save(publisher, null, cancellationToken);
        }

        public async Task Handle(OrderCloseCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.CloseOrder(request.TicketNo);
            await repository.Save(publisher, null, cancellationToken);
        }
    }
}
