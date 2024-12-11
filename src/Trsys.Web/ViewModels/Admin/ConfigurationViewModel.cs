using Trsys.Models;

namespace Trsys.Web.ViewModels.Admin
{
    public class ConfigurationViewModel
    {
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public EmailConfiguration EmailConfiguration { get; set; } = new EmailConfiguration();
    }
}
