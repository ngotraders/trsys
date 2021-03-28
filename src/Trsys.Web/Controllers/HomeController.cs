using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Web.ViewModels.Home;

namespace Trsys.Web.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginExecute(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "入力に誤りがあります。";
                return View("Login", model);
            }
            if (model.Username != "admin" || model.Password != "P@ssw0rd")
            {
                model.ErrorMessage = "ユーザー名またはパスワードが違います。";
                return View("Login", model);
            }

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, "admin"),
                new Claim(ClaimTypes.Name, "Administrator"),
                new Claim(ClaimTypes.Role, "Administrator"),
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(principal);
            return Redirect(returnUrl ?? "/");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/login");
        }
    }
}
