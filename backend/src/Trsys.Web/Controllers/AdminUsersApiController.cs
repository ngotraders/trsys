using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Queries;
using Trsys.Web.Identity;
using Trsys.Web.Requests;

namespace Trsys.Web.Controllers.Admin;

[Route("/api/admin/users")]
[ApiController]
[Authorize]
public class AdminUsersApiController(IMediator mediator, UserManager<TrsysUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<Ok<List<UserDto>>> Index(
        [FromQuery] int? _start,
        [FromQuery] int? _end,
        [FromQuery] string[]? _sort,
        [FromQuery] string[]? _order)
    {
        var response = await mediator.Send(new SearchUsers(_start, _end, _sort, _order));
        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();
        return TypedResults.Ok(response.Items);
    }

    [HttpPost]
    public async Task<Results<Ok<UserDto>, ValidationProblem>> Post([FromBody] CreateUserRequest request)
    {
        var identity = new TrsysUser()
        {
            Name = request.Name,
            UserName = request.Username,
            Email = request.EmailAddress,
            EmailConfirmed = true,
            Role = request.Role,
        };
        var result = await userManager.CreateAsync(identity, request.Password!);
        if (!result.Succeeded)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { "", result.Errors.Select(e => e.Description).ToArray() }
            });
        }
        return TypedResults.Ok(await mediator.Send(new GetUser(identity.Id)));
    }

    [HttpGet("{id}")]
    public async Task<Results<Ok<UserDto>, NotFound>> Get(Guid id)
    {
        var response = await mediator.Send(new GetUser(id));
        if (response == null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(response);
    }

    [HttpPatch("{id}")]
    public async Task<Results<Ok<UserDto>, NotFound, ValidationProblem>> Put(Guid id, [FromBody] UpdateUserRequest request)
    {
        var identity = await userManager.FindByIdAsync(id.ToString());
        if (identity == null)
        {
            return TypedResults.NotFound();
        }
        try
        {
            identity.UserName = request.Username;
            identity.Email = request.EmailAddress;
            identity.Name = request.Name;
            identity.Role = request.Role;
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                identity.PasswordHash = userManager.PasswordHasher.HashPassword(identity, request.NewPassword!);
            }
            var result = await userManager.UpdateAsync(identity!);
            if (!result.Succeeded)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "", result.Errors.Select(e => e.Description).ToArray() }
                });
            }
            return TypedResults.Ok(await mediator.Send(new GetUser(id)));
        }
        catch (InvalidOperationException e)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                { "", [e.Message] }
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<Results<Ok, NotFound, BadRequest<string>>> Delete(Guid id)
    {
        var identity = await userManager.FindByIdAsync(id.ToString());
        if (identity == null)
        {
            return TypedResults.NotFound();
        }
        var result = await userManager.DeleteAsync(identity);
        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.First().Description);
        }
        return TypedResults.Ok();
    }
}
