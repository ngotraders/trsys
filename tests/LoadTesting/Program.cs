using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoadTesting
{

    class Program
    {
        const int COUNT_OF_CLIENTS = 65;
        const double LENGTH_OF_TEST_MINUTES = 3;
        const string ENDPOINT_URL = "https://localhost:5001";

        static void Main(string[] _)
        {
            using var server = new ProcessRunner("dotnet", "Trsys.Web.dll");

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
                .WithClean(context => Task.WhenAll(publisher.FinalizeAsync()));

            var scenario2 = ScenarioBuilder
                .CreateScenario("sub", step2)
                .WithInit(context => Task.WhenAll(subscribers.Select(subscriber => subscriber.InitializeAsync())))
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInjectPerSec(10 * COUNT_OF_CLIENTS, TimeSpan.FromMinutes(LENGTH_OF_TEST_MINUTES)))
                .WithClean(context => Task.WhenAll(subscribers.Select(subscriber => subscriber.FinalizeAsync())));

            NBomberRunner
                .RegisterScenarios(scenario1, scenario2)
                .Run();
        }

        private static async Task<IEnumerable<string>> GenerateSecretKeys(int count)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(ENDPOINT_URL);
            await client.PostAsync("/login", new FormUrlEncodedContent(
                new KeyValuePair<string, string>[] {
                    KeyValuePair.Create("Username", "admin"),
                    KeyValuePair.Create("Password", "P@ssw0rd"),
                }));

            var secretKeys = await GetSecretKeysAsync(client);
            foreach (var secretKey in secretKeys)
            {
                await client.PostAsync($"/admin/keys/{secretKey}/revoke", new StringContent("", Encoding.UTF8, "text/plain"));
                await client.PostAsync($"/admin/keys/{secretKey}/delete", new StringContent("", Encoding.UTF8, "text/plain"));
            }

            for (var i = 0; i < count; i++)
            {
                await client.PostAsync("/admin/keys/new", new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[] {
                        KeyValuePair.Create("KeyType", "3"),
                    }));
            }

            secretKeys = await GetSecretKeysAsync(client);
            foreach (var secretKey in secretKeys)
            {
                await client.PostAsync($"/admin/keys/{secretKey}/approve", new StringContent("", Encoding.UTF8, "text/plain"));
            }
            return secretKeys;
        }

        private static async Task<IEnumerable<string>> GetSecretKeysAsync(HttpClient client)
        {
            var secretKeys = new List<string>();
            var adminRes = await client.GetAsync("/admin");
            var html = await adminRes.Content.ReadAsStringAsync();
            var index = html.IndexOf("<table");
            if (index >= 0)
            {
                index = html.IndexOf("<span class=\"secret-key\"", index);
            }
            while (index >= 0)
            {
                var endIndex = html.IndexOf("</span>", index);
                if (endIndex < 0)
                {
                    break;
                }
                var secretKey = html.Substring(index + 25, endIndex - (index + 25));
                secretKeys.Add(secretKey);
                index = html.IndexOf("<span class=\"secret-key\">", index + 1);
            }
            return secretKeys;
        }
    }
}

