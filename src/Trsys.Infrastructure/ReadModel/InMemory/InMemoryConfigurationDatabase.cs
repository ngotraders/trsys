using System.Threading.Tasks;
using Trsys.Models.Configurations;
using Trsys.Models.ReadModel.Infrastructure;

namespace Trsys.Infrastructure.ReadModel.InMemory;

public class InMemoryConfigurationDatabase : IConfigurationDatabase
{
    private EmailConfiguration emailConfiguration;


    public Task<EmailConfiguration> GetEmailConfigurationAsync()
    {
        return Task.FromResult(emailConfiguration ?? new EmailConfiguration());
    }

    public Task SaveEmailConfigurationAsync(EmailConfiguration emailConfiguration)
    {
        this.emailConfiguration = emailConfiguration;
        return Task.CompletedTask;
    }
}
