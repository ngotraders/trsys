using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Queries;

namespace Trsys.Web.Controllers.Admin
{
    [Route("/api/admin/users")]
    [Authorize(Roles = "Administrator")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IConfiguration configuration;

        public UsersApiController(IMediator mediator, IConfiguration configuration)
        {
            this.mediator = mediator;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<List<UserDto>> Index(int? _start, int? _end)
        {
            var response = await mediator.Send(new GetUsers(_start, _end));
            Response.Headers["X-Total-Count"] = response.TotalCount.ToString();
            return response.Items;
        }

        [HttpGet("{id}")]
        public async Task<UserDto> Get(Guid id)
        {
            var response = await mediator.Send(new GetUser(id));
            if (response == null)
            {
                throw new KeyNotFoundException();
            }
            return response;
        }
    }
}
