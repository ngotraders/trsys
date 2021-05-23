using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoadTesting
{

    class Program
    {
        const int COUNT_OF_CLIENTS = 65;
        const double LENGTH_OF_TEST_MINUTES = 3;
        const string ENDPOINT_URL = "https://localhost:5001";

        static void Main(string[] args)
        {
            using var server = Trsys.Web.Program.CreateHostBuilder(args).Build();
            server.StartAsync().Wait();

            var secretKeys = GenerateSecretKeys(COUNT_OF_CLIENTS + 1).Result;
            var feeds = Feed.CreateConstant("secret_keys", FeedData.FromSeq(secretKeys).ShuffleData());
            var orderProvider = new OrderProvider(TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES));
            var subscribers = Enumerable.Range(1, COUNT_OF_CLIENTS).Select(i => new Subscriber(ENDPOINT_URL, secretKeys.Skip(i).First())).ToList();
            var publisher = new Publisher(ENDPOINT_URL, secretKeys.First(), orderProvider);
            orderProvider.SetStart();

            var step1 = Step.Create("publisher", feeds, context => publisher.ExecuteAsync());
            var step2 = Step.Create("subscriber", feeds, context => subscribers[context.CorrelationId.CopyNumber % COUNT_OF_CLIENTS].ExecuteAsync());

            var scenario1 = ScenarioBuilder
                .CreateScenario("pub", step1)
                .WithInit(async context =>
                {
                    await publisher.InitializeAsync();
                    await publisher.ExecuteAsync();
                })
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInjectPerSec(1, TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES)))
                .WithClean(async context =>
                {
                    await Task.WhenAll(publisher.FinalizeAsync());
                    await DeleteSecretKeys(secretKeys.Take(1));
                });


            var scenario2 = ScenarioBuilder
                .CreateScenario("sub", step2)
                .WithInit(context => Task.WhenAll(subscribers.Select(subscriber => subscriber.InitializeAsync())))
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInjectPerSec(10 * COUNT_OF_CLIENTS, TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES)))
                .WithClean(async context =>
                {
                    await Task.WhenAll(subscribers.Select(subscriber => subscriber.FinalizeAsync()));
                    await DeleteSecretKeys(secretKeys.Skip(1));
                });

            NBomberRunner
                .RegisterScenarios(scenario1, scenario2)
                .Run();

        }

        private static async Task<IEnumerable<string>> GenerateSecretKeys(int count)
        {
            var admin = new Admin(ENDPOINT_URL, "admin", "P@ssw0rd");
            await admin.LoginAsync();

            var secretKeys = await admin.GetSecretKeysAsync();
            foreach (var secretKey in secretKeys)
            {
                await admin.RevokeSecretKeyAsync(secretKey);
                await admin.DeleteSecretKeyAsync(secretKey);
            }

            for (var i = 0; i < count; i++)
            {
                await admin.CreateKeyAsync();
            }

            secretKeys = await admin.GetSecretKeysAsync();
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

