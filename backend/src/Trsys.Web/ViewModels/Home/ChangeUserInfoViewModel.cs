using System.ComponentModel.DataAnnotations;

namespace Trsys.Web.ViewModels.Home
{
    public class ChangeUserInfoViewModel
    {
        public string ErrorMessage { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string EmailAddress { get; set; }
    }
}
