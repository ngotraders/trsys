using CQRSlite.Domain;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Domain;

namespace Trsys.Web.Models.WriteModel.Handlers
{
    public class FetchedOrderCommandHandlers :
        IRequestHandler<FetchOrderCommand>
    {
        private readonly IRepository repository;

        public FetchedOrderCommandHandlers(IRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Unit> Handle(FetchOrderCommand request, CancellationToken cancellationToken = default)
        {
            var publisher = await repository.Get<SecretKeyAggregate>(request.Id, cancellationToken);
            publisher.Subscribed(request.Tickets);
            await repository.Save(publisher, null, cancellationToken);
            return Unit.Value;
        }
    }
}
