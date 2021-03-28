using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public IActionResult Login(string returnUrl)
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginExecute(string returnUrl)
        {
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
