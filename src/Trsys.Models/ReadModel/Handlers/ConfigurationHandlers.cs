using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models.Events;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Infrastructure;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Models.ReadModel.Handlers;

public class ConfigurationHandlers :
    INotificationHandler<WorldStateConfigurationUpdated>,
    IRequestHandler<GetConfiguration, ConfigurationDto>
{
    private readonly IConfigurationDatabase db;

    public ConfigurationHandlers(IConfigurationDatabase db)
    {
        this.db = db;
    }

    public Task Handle(WorldStateConfigurationUpdated notification, CancellationToken cancellationToken)
    {
        return db.SaveEmailConfigurationAsync(notification.EmailConfiguration);
    }

    public async Task<ConfigurationDto> Handle(GetConfiguration request, CancellationToken cancellationToken)
    {
        var emailConfiguration = await db.GetEmailConfigurationAsync();
        if (emailConfiguration == null)
        {
            return new ConfigurationDto();
        }
        return new ConfigurationDto()
        {
            EmailConfiguration = emailConfiguration,
        };
    }
}
