using System.ComponentModel.DataAnnotations;

namespace Trsys.Web.ViewModels.Home
{
    public class ChangePasswordViewModel
    {
        public string ErrorMessage { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordConfirm { get; set; }
    }
}
