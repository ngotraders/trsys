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

            var feeds = Feed.CreateConstant("secret_keys", FeedData.FromSeq(GenerateSecretKeys(1).Result));
            var step = Step.Create("subscriber", feeds, async context =>
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:5001");
                var res = await client.PostAsync("/api/token", new StringContent(context.FeedItem, Encoding.UTF8, "text/plain"));
                if (!res.IsSuccessStatusCode)
                {
                    context.Logger.Error($"POST /api/token finished unsuccessful status code: {res.StatusCode}");
                    var success = false;
                    for (var i = 0; i < 100; i++)
                    {
                        res = await client.PostAsync("/api/token", new StringContent(context.FeedItem, Encoding.UTF8, "text/plain"));
                        if (res.IsSuccessStatusCode)
                        {
                            success = true;
                            break;
                        }
                        context.Logger.Error($"POST /api/token finished unsuccessful status code: {res.StatusCode}");
                        await Task.Delay(100);
                    }
                    if (!success)
                    {
                        return Response.Fail();
                    }
                }
                var secretToken = await res.Content.ReadAsStringAsync();
                client.DefaultRequestHeaders.Add("X-Secret-Token", secretToken);
                for (var i = 0; i < 1000; i++)
                {
                    var res2 = await client.GetAsync("/api/orders");
                    if (!res2.IsSuccessStatusCode)
                    {
                        context.Logger.Error($"GET /api/orders finished unsuccessful status code: {res2.StatusCode}");
                    }
                    await Task.Delay(100);
                }
                return Response.Ok();
            });

            var scenario = ScenarioBuilder.CreateScenario("sub", step);

            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }

        private static async Task<IEnumerable<string>> GenerateSecretKeys(int count)
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
            var index = html.IndexOf("<table>");
            index = html.IndexOf("<td class=\"secret-key\"", index);
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

