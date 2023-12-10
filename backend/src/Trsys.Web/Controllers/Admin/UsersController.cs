using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Models;
using Trsys.Models.ReadModel.Dtos;
using Trsys.Models.ReadModel.Queries;
using Trsys.Models.WriteModel.Commands;
using Trsys.Web.ViewModels.Admin;

namespace Trsys.Web.Controllers.Admin
{
    [Route("/api/admin/users")]
    [Authorize(Roles = "Administrator")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IConfiguration configuration;

        public UsersController(IMediator mediator, IConfiguration configuration)
        {
            this.mediator = mediator;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<List<UserDto>> Index(int? _start, int? _end)
        {
            var response = await mediator.Send(new GetUsers(_start, _end));
            Response.Headers.Add("X-Total-Count", response.TotalCount.ToString());
            return response.Items;
        }
    }
}
