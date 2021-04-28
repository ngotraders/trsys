using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Web.Authentication;
using Trsys.Web.Models.Users;
using Trsys.Web.ViewModels.Home;

namespace Trsys.Web.Controllers
{
    [Route("/")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserRepository repository;
        private readonly PasswordHasher passwordHasher;

        public HomeController(IUserRepository repository, PasswordHasher passwordHasher)
        {
            this.repository = repository;
            this.passwordHasher = passwordHasher;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect(returnUrl ?? "/");
            }
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginExecute(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "入力に誤りがあります。";
                return View("Login", model);
            }

            var user = await repository.FindByUsernameAsync(model.Username);
            if (user == null || user.Password != passwordHasher.Hash(model.Password))
            {
                model.ErrorMessage = "ユーザー名またはパスワードが違います。";
                return View("Login", model);
            }

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(principal);
            return Redirect(returnUrl ?? "/");
        }

        [HttpGet("changePassword")]
        public IActionResult ChangePassword()
        {
            var model = new ChangePasswordViewModel();
            return View(model);
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePasswordExecute(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "入力に誤りがあります。";
                return View("ChangePassword", model);
            }
            if (model.NewPassword != model.NewPasswordConfirm)
            {
                model.ErrorMessage = "確認用パスワードが違います。";
                return View("ChangePassword", model);
            }

            var user = await repository.FindByUsernameAsync(User.Identity.Name);
            user.Password = passwordHasher.Hash(model.NewPassword);
            await repository.SaveAsync(user);

            return Redirect("/");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/login");
        }
    }
}
