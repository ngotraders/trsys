using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;
using Trsys.Web.Configurations;
using Trsys.Web.ViewModels.Home;

namespace Trsys.Web.Controllers
{
    [Route("/")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IMediator mediator;
        private readonly PasswordHasher passwordHasher;

        public HomeController(IMediator mediator, PasswordHasher passwordHasher)
        {
            this.mediator = mediator;
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

            var user = await mediator.Send(new FindByUsername(model.Username));
            if (user == null || user.PasswordHash != passwordHasher.Hash(model.Password))
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

        [HttpGet("changeUserInfo")]
        public IActionResult ChangeUserInfo()
        {
            var model = new ChangeUserInfoViewModel();
            return View(model);
        }

        [HttpPost("changeUserInfo")]
        public async Task<IActionResult> ChangeUserInfoExecute(ChangeUserInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ErrorMessage = "入力に誤りがあります。";
                return View("ChangeUserInfo", model);
            }

            var user = await mediator.Send(new FindByUsername(User.Identity.Name));
            if (user == null)
            {
                model.ErrorMessage = "予期せぬエラーが発生しました。";
                return View("ChangeUserInfo", model);
            }
            await mediator.Send(new UserUpdateCommand(user.Id, model.Name, model.EmailAddress));
            return Redirect("/");
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

            var user = await mediator.Send(new FindByUsername(User.Identity.Name));
            if (user == null)
            {
                model.ErrorMessage = "予期せぬエラーが発生しました。";
                return View("ChangePassword", model);
            }
            await mediator.Send(new UserChangePasswordHashCommand(user.Id, passwordHasher.Hash(model.NewPassword)));
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
