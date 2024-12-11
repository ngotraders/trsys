using MediatR;

namespace Trsys.Models.WriteModel.Commands;

public class ConfigurationUpdateCommand : IRequest, IRetryableRequest
{
    public ConfigurationUpdateCommand(EmailConfiguration configuration)
    {
        EmailConfiguration = configuration;
    }

    public EmailConfiguration EmailConfiguration { get; }
}
