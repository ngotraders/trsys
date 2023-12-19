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

[Route("/api/admin/trade-histories")]
[ApiController]
[Authorize]
public class AdminTradeHistorysApiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<Ok<List<TradeHistoryDto>>> Index(
        [FromQuery] int? _start,
        [FromQuery] int? _end,
        [FromQuery] string[]? _sort,
        [FromQuery] string[]? _order)
    {
        var response = await mediator.Send(new SearchTradeHistories(_start, _end, _sort, _order));
        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();
        return TypedResults.Ok(response.Items);
    }

    [HttpGet("{id}")]
    public async Task<Results<Ok<TradeHistoryDto>, NotFound>> Get(string id)
    {
        var response = await mediator.Send(new GetTradeHistory(id));
        if (response == null)
        {
            return TypedResults.NotFound();
        }
        return TypedResults.Ok(response);
    }
}
