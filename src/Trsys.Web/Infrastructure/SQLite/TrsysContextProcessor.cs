using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Data;

namespace Trsys.Web.Infrastructure.SQLite
{
    public class TrsysContextProcessor : IDisposable
    {
        private readonly BlockingCollection<Func<TrsysContext, Task>> _queue = new();
        private readonly IServiceScope scope;
        private readonly TrsysContext db;
        private readonly Task task;

        private bool disposedValue;

        public TrsysContextProcessor(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime applicationLifetime)
        {
            scope = serviceScopeFactory.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<TrsysContext>();
            this.task = Task.Run(async () => await Process(applicationLifetime.ApplicationStopping));
        }

        public async Task Process(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var process = _queue.Take(token);
                await process.Invoke(db);
            }
        }

        public Task Enqueue(Func<TrsysContext, Task> process)
        {
            var tcs = new TaskCompletionSource<object>();
            _queue.Add(async db =>
            {
                try
                {
                    await process(db);
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            return tcs.Task;
        }

        public Task<T> Enqueue<T>(Func<TrsysContext, Task<T>> process)
        {
            var tcs = new TaskCompletionSource<T>();
            _queue.Add(async db =>
            {
                try
                {
                    var result = await process(db);
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }

            });
            return tcs.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    db.Dispose();
                    scope.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
