using NBomber;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
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
        const int COUNT_OF_CLIENTS = 60;
        const string ENDPOINT_URL = "https://localhost:5001";
        const string ORDER_DATA = "1:USDJPY:0@2:EURUSD:1";

        static void Main(string[] _)
        {
            using var server = new ProcessRunner("dotnet", "Trsys.Web.dll");

            var secretTokens = GenerateSecretTokens(COUNT_OF_CLIENTS).Result;
            SetPublisherData(ORDER_DATA, secretTokens.FirstOrDefault()).Wait();
            var feeds = Feed.CreateConstant("secret_keys", FeedData.FromSeq(secretTokens).ShuffleData());
            var step = HttpStep.Create("subscriber", feeds,
                context =>
                {
                    return Http.CreateRequest("GET", ENDPOINT_URL + "/api/orders")
                        .WithHeader("X-Secret-Token", context.FeedItem)
                        .WithCheck(async res =>
                        {
                            if (!res.IsSuccessStatusCode)
                            {
                                return Response.Fail($"Not successful status code: {res.StatusCode}");
                            }
                            var responseText = await res.Content.ReadAsStringAsync();
                            if (responseText != ORDER_DATA)
                            {
                                return Response.Fail($"Invalid response: {responseText}");
                            }
                            return Response.Ok();
                        });
                });


            var scenario = ScenarioBuilder
                .CreateScenario("sub", step)
                .WithWarmUpDuration(TimeSpan.FromSeconds(5))
                .WithLoadSimulations(LoadSimulation.NewInjectPerSec(10 * COUNT_OF_CLIENTS, TimeSpan.FromMinutes(3)));

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }

        private static async Task<IEnumerable<string>> GenerateSecretTokens(int count)
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

        private static async Task SetPublisherData(string data, string secretToken)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(ENDPOINT_URL);
            client.DefaultRequestHeaders.Add("X-Secret-Token", secretToken);
            var res = await client.PostAsync("/api/orders", new StringContent(data, Encoding.UTF8, "text/plain"));
            res.EnsureSuccessStatusCode();
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

