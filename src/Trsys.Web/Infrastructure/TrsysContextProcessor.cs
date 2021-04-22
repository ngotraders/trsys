using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Trsys.Web.Data;

namespace Trsys.Web.Infrastructure
{
    public class TrsysContextProcessor
    {
        private readonly BlockingCollection<Func<TrsysContext, Task>> _queue = new();
        private readonly TrsysContext db;
        private readonly Task task;

        public TrsysContextProcessor(TrsysContext db, IHostApplicationLifetime applicationLifetime)
        {
            this.db = db;
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
    }
}
