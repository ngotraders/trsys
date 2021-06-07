using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trsys.Web.Infrastructure.Queue
{
    public class BlockingTaskQueue : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1);

        public async Task Enqueue(Action action)
        {
            await semaphore.WaitAsync();
            try
            {
                action.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Enqueue(Func<Task> function)
        {
            await semaphore.WaitAsync();
            try
            {
                await function.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<T> Enqueue<T>(Func<T> function)
        {
            await semaphore.WaitAsync();
            try
            {
                return function.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> function)
        {
            await semaphore.WaitAsync();
            try
            {
                return await function.Invoke();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            semaphore.Wait();
            semaphore.Dispose();
        }
    }
}
