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
    [Route("/api/auth")]
    [Authorize]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly PasswordHasher passwordHasher;

        public AuthApiController(IMediator mediator, PasswordHasher passwordHasher)
        {
            this.mediator = mediator;
            this.passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await mediator.Send(new FindByUsername(model.Username));
            if (user == null)
            {
                return BadRequest();
            }
            var result = await mediator.Send(new GetUserPasswordHash(user.Id));
            if (result.PasswordHash != passwordHasher.Hash(model.Password))
            {
                return BadRequest();
            }

            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            }, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(principal);
            return NoContent();
        }

        [HttpGet("userInfo")]
        public async Task<UserInfoViewModel> UserInfo()
        {
            var user = await mediator.Send(new FindByUsername(User.Identity.Name));
            var model = new UserInfoViewModel()
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Role = user.Role,
                EmailAddress = user.EmailAddress,
            };
            return model;
        }

        [HttpPost("changeUserInfo")]
        public async Task<IActionResult> ChangeUserInfo(ChangeUserInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await mediator.Send(new FindByUsername(User.Identity.Name));
            if (user == null)
            {
                return StatusCode(500);
            }
            await mediator.Send(new UserUpdateUserInfoCommand(user.Id, model.Name, model.EmailAddress));
            return NoContent();
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (model.NewPassword != model.NewPasswordConfirm)
            {
                return BadRequest();
            }

            var user = await mediator.Send(new FindByUsername(User.Identity.Name));
            if (user == null)
            {
                return StatusCode(500);
            }
            await mediator.Send(new UserChangePasswordHashCommand(user.Id, passwordHasher.Hash(model.NewPassword)));
            return NoContent();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return NoContent();
        }
    }
}
