using System.Threading.Tasks;
using Trsys.Models.Configurations;

namespace Trsys.Models.ReadModel.Infrastructure;

public interface IConfigurationDatabase
{
    Task<EmailConfiguration> GetEmailConfigurationAsync();
    Task SaveEmailConfigurationAsync(EmailConfiguration emailConfiguration);
}
