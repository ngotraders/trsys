using Microsoft.AspNetCore.Hosting;
using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoadTesting
{

    class Program
    {
        static void Main(string[] _)
        {
            using var server = new ProcessRunner("dotnet", "Trsys.Web.dll");

            const int countOfClients = 10;
            var feeds = Feed.CreateConstant("secret_keys", FeedData.FromSeq(GenerateSecretTokens(countOfClients).Result).ShuffleData());
            var step = Step.Create("subscriber", feeds, async context =>
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:5001");
                client.DefaultRequestHeaders.Add("X-Secret-Token", context.FeedItem);
                var res = await client.GetAsync("/api/orders");
                if (!res.IsSuccessStatusCode)
                {
                    context.Logger.Error($"GET /api/orders finished unsuccessful status code: {res.StatusCode}");
                    return Response.Fail();
                }
                return Response.Ok();
            });

            var scenario = ScenarioBuilder
                .CreateScenario("sub", step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInjectPerSec(10 * countOfClients, TimeSpan.FromMinutes(3)));

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }

        private static async Task<IEnumerable<string>> GenerateSecretTokens(int count)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:5001");
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
                        KeyValuePair.Create("KeyType", "2"),
                    }));
            }

            secretKeys = await GetSecretKeysAsync(client);

            var secretTokens = new List<string>();
            foreach (var secretKey in secretKeys)
            {
                await client.PostAsync($"/admin/keys/{secretKey}/approve", new StringContent("", Encoding.UTF8, "text/plain"));
                var res = await client.PostAsync("/api/token", new StringContent(secretKey, Encoding.UTF8, "text/plain"));
                res.EnsureSuccessStatusCode();
                secretTokens.Add(await res.Content.ReadAsStringAsync());
            }

            return secretTokens;
        }

        private static async Task<IEnumerable<string>> GetSecretKeysAsync(HttpClient client)
        {
            var secretKeys = new List<string>();
            var adminRes = await client.GetAsync("/admin");
            var html = await adminRes.Content.ReadAsStringAsync();
            var index = html.IndexOf("<table>");
            if (index >= 0)
            {
                index = html.IndexOf("<td class=\"secret-key\"", index);
            }
            while (index >= 0)
            {
                var secretKey = html.Substring(index + 23, 36);
                secretKeys.Add(secretKey);
                index = html.IndexOf("<td class=\"secret-key\">", index + 1);
            }
            return secretKeys;
        }
    }
}

