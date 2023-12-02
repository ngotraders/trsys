using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Data.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTesting
{

    class Program
    {
        const int COUNT_OF_CLIENTS = 100;
        const double LENGTH_OF_TEST_MINUTES = 3;
        const string ENDPOINT_URL = "https://localhost:44326";

        static void Main(string[] _)
        {
            //// using var server = Trsys.Web.Program.CreateHostBuilder(args).Build();
            //// server.StartAsync().Wait();
            // using var server = new ProcessRunner("dotnet", "Trsys.Web.dll");

            var secretKeys = WithRetry(() => GenerateSecretKeys(COUNT_OF_CLIENTS + 1)).Result;
            var feeds = DataFeed.Constant(secretKeys);
            var orderProvider = new OrderProvider(TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES));
            var subscribers = Enumerable.Range(1, COUNT_OF_CLIENTS).Select(i => new Subscriber(ENDPOINT_URL, secretKeys.Skip(i).First())).ToList();
            var publisher = new Publisher(ENDPOINT_URL, secretKeys.First(), orderProvider);
            orderProvider.SetStart();

            var scenario1 = Scenario
                .Create("publisher", context => publisher.ExecuteAsync())
                .WithInit(async context =>
                {
                    await publisher.InitializeAsync();
                    await publisher.ExecuteAsync();
                })
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInject(1, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES)))
                .WithClean(async context =>
                {
                    await Task.WhenAll(publisher.FinalizeAsync());
                    await DeleteSecretKeys(secretKeys.Take(1));
                });


            var scenario2 = Scenario
                .Create("subscriber", context => subscribers[context.InvocationNumber % COUNT_OF_CLIENTS].ExecuteAsync())
                .WithInit(context => Task.WhenAll(subscribers.Select(subscriber => subscriber.InitializeAsync())))
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInject(10 * COUNT_OF_CLIENTS, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES)))
                .WithClean(async context =>
                {
                    await Task.WhenAll(subscribers.Select(subscriber => subscriber.FinalizeAsync()));
                    await DeleteSecretKeys(secretKeys.Skip(1));
                });

            NBomberRunner
                .RegisterScenarios(scenario1, scenario2)
                .Run();

        }

        private static async Task<T> WithRetry<T>(Func<Task<T>> func)
        {
            int retryCount = 0;
            Exception lastException = null;
            while (retryCount < 10)
            {
                try
                {
                    return await Task.Run(async () => await func());
                }
                catch (Exception e)
                {
                    lastException = e;
                    Thread.Sleep(1000);
                    retryCount++;
                }
            }
            throw new Exception("Failed to execute.", lastException);
        }

        private static async Task<IEnumerable<string>> GenerateSecretKeys(int count)
        {
            var admin = new Admin(ENDPOINT_URL, "admin", "P@ssw0rd");
            await admin.LoginAsync();

            var secretKeys = (await admin.GetSecretKeysAsync())
                .Where(k => Guid.TryParse(k, out var _))
                .ToList();
            foreach (var secretKey in secretKeys)
            {
                await admin.RevokeSecretKeyAsync(secretKey);
                await admin.DeleteSecretKeyAsync(secretKey);
            }

            for (var i = 0; i < count; i++)
            {
                await admin.CreateKeyAsync();
            }

            secretKeys = (await admin.GetSecretKeysAsync())
                .Where(k => Guid.TryParse(k, out var _))
                .ToList();
            foreach (var secretKey in secretKeys)
            {
                await admin.ApproveSecretKeyAsync(secretKey);
            }
            return secretKeys;
        }

        private static async Task DeleteSecretKeys(IEnumerable<string> secretKeys)
        {
            var admin = new Admin(ENDPOINT_URL, "admin", "P@ssw0rd");
            await admin.LoginAsync();
            foreach (var secretKey in secretKeys)
            {
                await admin.RevokeSecretKeyAsync(secretKey);
                await admin.DeleteSecretKeyAsync(secretKey);
            }
        }
    }
}

