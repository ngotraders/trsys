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
    public async Task<Ok<List<UserDto>>> Index(int? _start, int? _end)
    {
        var response = await mediator.Send(new GetUsers(_start, _end));
        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();
        return TypedResults.Ok(response.Items);
    }

    [HttpPost]
    public async Task<Results<Ok<UserDto>, ValidationProblem, BadRequest<string>>> Post([FromBody] CreateUserRequest request)
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
            return TypedResults.BadRequest(result.Errors.First().Description);
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
}
