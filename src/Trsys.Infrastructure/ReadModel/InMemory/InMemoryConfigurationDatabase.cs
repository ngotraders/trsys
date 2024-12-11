using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory;

public class InMemoryConfigurationDatabase : IConfigurationDatabase
{
    private EmailConfiguration emailConfiguration;


    public Task<EmailConfiguration> GetEmailConfigurationAsync()
    {
        return Task.FromResult(emailConfiguration);
    }

    public Task SaveEmailConfigurationAsync(EmailConfiguration emailConfiguration)
    {
        this.emailConfiguration = emailConfiguration;
        return Task.CompletedTask;
    }
}
