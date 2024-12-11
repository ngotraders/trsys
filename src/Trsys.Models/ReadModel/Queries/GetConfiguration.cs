﻿using MediatR;
using Trsys.Models.ReadModel.Dtos;

namespace Trsys.Models.ReadModel.Queries
{
    public class GetConfiguration : IRequest<ConfigurationDto>
    {
    }
}
