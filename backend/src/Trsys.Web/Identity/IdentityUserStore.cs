using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Identity
{
    internal class IdentityUserStore(IMediator mediator, ILogger<IdentityUserStore> logger) : IUserStore<IdentityUser>, IUserPasswordStore<IdentityUser>, IUserEmailStore<IdentityUser>
    {
        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            try
            {
                await mediator.Send(new UserCreateCommand(user.UserName, user.UserName, user.Email, user.PasswordHash, "Normal"), cancellationToken);
                return IdentityResult.Success;

            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create user");
                return IdentityResult.Failed();
            }
        }

        public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var userDto = await mediator.Send(new FindByNormalizedUsername(normalizedEmail), cancellationToken);
            if (userDto == null)
            {
                return null;
            }
            var passwordHashDto = await mediator.Send(new GetUserPasswordHash(userDto.Id), cancellationToken);
            return new IdentityUser(userDto.Username)
            {
                Id = userDto.Id.ToString(),
                Email = userDto.EmailAddress,
                EmailConfirmed = true,
                NormalizedEmail = userDto.EmailAddress.ToUpperInvariant(),
                NormalizedUserName = userDto.Username.ToUpperInvariant(),
                PasswordHash = passwordHashDto.PasswordHash,
                UserName = userDto.Username,
            };
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var userDto = await mediator.Send(new GetUser(Guid.Parse(userId)), cancellationToken);
            if (userDto == null)
            {
                return null;
            }
            var passwordHashDto = await mediator.Send(new GetUserPasswordHash(userDto.Id), cancellationToken);
            return new IdentityUser(userDto.Username)
            {
                Id = userDto.Id.ToString(),
                Email = userDto.EmailAddress,
                EmailConfirmed = true,
                NormalizedEmail = userDto.EmailAddress.ToUpperInvariant(),
                NormalizedUserName = userDto.Username.ToUpperInvariant(),
                PasswordHash = passwordHashDto.PasswordHash,
                UserName = userDto.Username,
            };
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var userDto = await mediator.Send(new FindByNormalizedUsername(normalizedUserName), cancellationToken);
            if (userDto == null)
            {
                return null;
            }
            var passwordHashDto = await mediator.Send(new GetUserPasswordHash(userDto.Id), cancellationToken);
            return new IdentityUser(userDto.Username)
            {
                Id = userDto.Id.ToString(),
                Email = userDto.EmailAddress,
                EmailConfirmed = true,
                NormalizedEmail = userDto.EmailAddress.ToUpperInvariant(),
                NormalizedUserName = userDto.Username.ToUpperInvariant(),
                PasswordHash = passwordHashDto.PasswordHash,
                UserName = userDto.Username,
            };
        }

        public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}