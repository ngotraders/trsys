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

[Route("/api/admin/keys")]
[ApiController]
[Authorize]
public class AdminKeysApiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Ok<List<SecretKeyDto>>> Index(int? _start, int? _end)
    {
        var response = await mediator.Send(new GetSecretKeys(_start, _end));
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
}
