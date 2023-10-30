using CQRSlite.Domain.Exception;
using MediatR;
using Microsoft.Extensions.Logging;
using SqlStreamStore.Streams;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Models;

namespace Trsys.Infrastructure.WriteModel
{
    public class RetryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRetryableRequest
    {
        private readonly ILogger<RetryPipelineBehavior<TRequest, TResponse>> logger;

        public RetryPipelineBehavior(ILogger<RetryPipelineBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (ConcurrencyException e)
            {
                logger.LogDebug(e, "retrying: {@request}", request);
                var result = await next();
                logger.LogDebug(e, "retrying: {@request}", request);
                return result;
            }
            catch (WrongExpectedVersionException e)
            {
                logger.LogDebug(e, "retrying: {@request}", request);
                var result = await next();
                logger.LogDebug(e, "retrying: {@request}", request);
                return result;
            }
        }
    }
}
