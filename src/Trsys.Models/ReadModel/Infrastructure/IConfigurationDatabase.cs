using System.Threading.Tasks;

namespace Trsys.Models.ReadModel.Infrastructure;

public interface IConfigurationDatabase
{
    Task<EmailConfiguration> GetEmailConfigurationAsync();
    Task SaveEmailConfigurationAsync(EmailConfiguration emailConfiguration);
}
