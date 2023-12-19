using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Identity;

internal class TrsysUserStore(IMediator mediator, ILogger<TrsysUserStore> logger) : IUserStore<TrsysUser>, IUserPasswordStore<TrsysUser>, IUserEmailStore<TrsysUser>
{
    public async Task<IdentityResult> CreateAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        try
        {
            var id = await mediator.Send(new UserCreateCommand(user.Name ?? user.UserName, user.UserName, user.Email, user.PasswordHash, user.Role), cancellationToken);
            user.Id = id;
            return IdentityResult.Success;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create user");
            return IdentityResult.Failed();
        }
    }

    public Task<IdentityResult> DeleteAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public async Task<TrsysUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var userDto = await mediator.Send(new FindByNormalizedUsername(normalizedEmail), cancellationToken);
        if (userDto == null)
        {
            return null;
        }
        var passwordHashDto = await mediator.Send(new GetUserPasswordHash(userDto.Id), cancellationToken);
        return new TrsysUser(userDto.Username)
        {
            Id = userDto.Id,
            Email = userDto.EmailAddress,
            EmailConfirmed = true,
            NormalizedEmail = userDto.EmailAddress.ToUpperInvariant(),
            NormalizedUserName = userDto.Username.ToUpperInvariant(),
            PasswordHash = passwordHashDto.PasswordHash,
            UserName = userDto.Username,
            Name = userDto.Name,
            Role = userDto.Role,
        };
    }

    public async Task<TrsysUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var userDto = await mediator.Send(new GetUser(Guid.Parse(userId)), cancellationToken);
        if (userDto == null)
        {
            return null;
        }
        var passwordHashDto = await mediator.Send(new GetUserPasswordHash(userDto.Id), cancellationToken);
        return new TrsysUser(userDto.Username)
        {
            Id = userDto.Id,
            Email = userDto.EmailAddress,
            EmailConfirmed = true,
            NormalizedEmail = userDto.EmailAddress.ToUpperInvariant(),
            NormalizedUserName = userDto.Username.ToUpperInvariant(),
            PasswordHash = passwordHashDto.PasswordHash,
            UserName = userDto.Username,
            Name = userDto.Name,
            Role = userDto.Role,
        };
    }

    public async Task<TrsysUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var userDto = await mediator.Send(new FindByNormalizedUsername(normalizedUserName), cancellationToken);
        if (userDto == null)
        {
            return null;
        }
        var passwordHashDto = await mediator.Send(new GetUserPasswordHash(userDto.Id), cancellationToken);
        return new TrsysUser(userDto.Username)
        {
            Id = userDto.Id,
            Email = userDto.EmailAddress,
            EmailConfirmed = true,
            NormalizedEmail = userDto.EmailAddress.ToUpperInvariant(),
            NormalizedUserName = userDto.Username.ToUpperInvariant(),
            PasswordHash = passwordHashDto.PasswordHash,
            UserName = userDto.Username,
            Name = userDto.Name,
            Role = userDto.Role,
        };
    }

    public Task<string?> GetEmailAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task<string?> GetNormalizedUserNameAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string?> GetPasswordHashAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<string> GetUserIdAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task<bool> HasPasswordAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetEmailAsync(TrsysUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task SetEmailConfirmedAsync(TrsysUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(TrsysUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    public Task SetNormalizedUserNameAsync(TrsysUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetPasswordHashAsync(TrsysUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(TrsysUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(TrsysUser user, CancellationToken cancellationToken)
    {
        var userDto = await mediator.Send(new GetUser(user.Id), cancellationToken);
        if (userDto == null)
        {
            return IdentityResult.Failed();
        }
        await mediator.Send(new UserUpdateCommand(user.Id, user.Name, user.UserName, user.Email, user.PasswordHash, user.Role), cancellationToken);
        return IdentityResult.Success;
    }
}