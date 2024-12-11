using CQRSlite.Domain;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Commands;
using Trsys.Models.WriteModel.Extensions;

namespace Trsys.Models.WriteModel.Handlers;

public class ConfigurationCommandHandler :
    IRequestHandler<ConfigurationUpdateCommand>
{
    private readonly IRepository repository;

    public ConfigurationCommandHandler(IRepository repository)
    {
        this.repository = repository;
    }

    public async Task Handle(ConfigurationUpdateCommand request, CancellationToken cancellationToken = default)
    {
        var worldState = await repository.GetWorldState();
        worldState.SaveEmailConfiguration(request.EmailConfiguration);
        await repository.Save(worldState, null, cancellationToken);
    }
}
