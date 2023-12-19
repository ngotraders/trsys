using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;
using Trsys.Web.Requests;

namespace Trsys.Web.Controllers.Admin;

[Route("/api/admin/secret-keys")]
[ApiController]
[Authorize]
public class AdminSecretKeysApiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Ok<List<SecretKeyDto>>> Index(
        [FromQuery] int? _start,
        [FromQuery] int? _end,
        [FromQuery] string[]? _sort,
        [FromQuery] string[]? _order)
    {
        var response = await mediator.Send(new SearchSecretKeys(_start, _end, _sort, _order));
        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();
        return TypedResults.Ok(response.Items);
    }

    [HttpPost]
    public async Task<Results<Ok<SecretKeyDto>, ValidationProblem>> Post([FromBody] CreateSecretKeyRequest request)
    {
        var response = await mediator.Send(new SecretKeyCreateCommand(request.KeyType, request.Key, request.Description, request.IsApproved));
        return TypedResults.Ok(await mediator.Send(new GetSecretKey(response)));
    }

    [HttpGet("{id}")]
    public async Task<Results<Ok<SecretKeyDto>, NotFound>> Get(Guid id)
    {
        var response = await mediator.Send(new GetSecretKey(id));
        if (response == null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(response);
    }

    [HttpPatch("{id}")]
    public async Task<Results<Ok<SecretKeyDto>, NotFound, ValidationProblem>> Put(Guid id, [FromBody] UpdateSecretKeyRequest request)
    {
        var response = await mediator.Send(new GetSecretKey(id));
        if (response == null)
        {
            return TypedResults.NotFound();
        }
        await mediator.Send(new SecretKeyUpdateCommand(id, request.KeyType, request.Description, request.IsApproved));
        return TypedResults.Ok(await mediator.Send(new GetSecretKey(id)));
    }

    [HttpDelete("{id}")]
    public async Task<Results<Ok, NotFound, ValidationProblem>> Delete(Guid id)
    {
        var response = await mediator.Send(new GetSecretKey(id));
        if (response == null)
        {
            return TypedResults.NotFound();
        }
        await mediator.Send(new SecretKeyDeleteCommand(id));
        return TypedResults.Ok();
    }
}
