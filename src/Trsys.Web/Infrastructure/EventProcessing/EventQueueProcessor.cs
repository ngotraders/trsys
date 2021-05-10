using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Models.Events;

namespace Trsys.Web.Infrastructure.EventProcessing
{
    public class EventQueueProcessor : BackgroundService
    {
        private readonly EventQueue queue;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public EventQueueProcessor(EventQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            this.queue = queue;
            this.serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var ev = await queue.DequeueAsync(stoppingToken);
                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                        await repository.SaveAsync(ev);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
