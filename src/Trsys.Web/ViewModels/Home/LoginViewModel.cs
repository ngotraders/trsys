using System.ComponentModel.DataAnnotations;

namespace Trsys.Web.ViewModels
{
    public class LoginViewModel
    {
        public string ErrorMessage { get; set; }

        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
