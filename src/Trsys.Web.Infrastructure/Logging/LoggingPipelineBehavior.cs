using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.Logging
{
    public class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger;

        public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var id = Guid.NewGuid();
            logger.LogDebug("processing {id}: {@request}", id, request);
            try
            {
                var response = await next();
                logger.LogDebug("processed {id}: {@response}", id, response);
                return response;
            }
            catch (Exception e)
            {
                logger.LogError(e, "error {id}", id);
                throw;
            }
        }
    }
}
