using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Web.Controllers.Admin;

[Route("/api/admin/events")]
[ApiController]
[Authorize]
public class AdminEventsApiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Ok<List<EventDto>>> Index(
        [FromQuery] int? _start,
        [FromQuery] int? _end,
        [FromQuery] string? source)
    {
        var response = await mediator.Send(new SearchEvents(_start, _end, source));
        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();
        return TypedResults.Ok(response.Items);
    }
}
