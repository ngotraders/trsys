using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Infrastructure.EventProcessing
{
    public class EventQueueProcessor : BackgroundService
    {
        private readonly EventQueue queue;
        private readonly IServiceScope scope;
        private readonly IEventRepository repository;
        private readonly ILogger<EventQueueProcessor> logger;

        public EventQueueProcessor(EventQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            this.queue = queue;
            this.scope = serviceScopeFactory.CreateScope();
            this.repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            this.logger = scope.ServiceProvider.GetRequiredService<ILogger<EventQueueProcessor>>();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var ev = await queue.DequeueAsync(stoppingToken);
                    await repository.SaveAsync(ev);
                    logger.LogInformation(ev.EventType + "::" + ev.Data);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            scope.Dispose();
        }
    }
}
