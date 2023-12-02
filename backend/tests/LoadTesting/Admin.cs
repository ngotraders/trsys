using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoadTesting
{
    public class Admin
    {
        private readonly HttpClient client;
        private readonly string username;
        private readonly string password;

        public Admin(string endpointUrl, string username, string password)
        {
            client = HttpClientFactory.Create(endpointUrl, true);
            client.BaseAddress = new Uri(endpointUrl);
            this.username = username;
            this.password = password;
        }

        public async Task LoginAsync()
        {
            await client.PostAsync("/login", new FormUrlEncodedContent(
                new KeyValuePair<string, string>[] {
                    KeyValuePair.Create("Username", username),
                    KeyValuePair.Create("Password", password),
                }));
        }

        public async Task<IEnumerable<string>> GetSecretKeysAsync()
        {
            var response = await client.GetAsync("/api/keys");
            var arr = JArray.Parse(await response.Content.ReadAsStringAsync());
            return arr.Select(e => e.Value<string>("key"));
        }

        public async Task<string> CreateKeyAsync(string secretKey = default)
        {
            var response = await client.PostAsync("/api/keys", new StringContent(JsonConvert.SerializeObject(new
            {
                Key = secretKey,
                KeyType = 3,
            }), Encoding.UTF8, "application/json"));
            var obj = JObject.Parse(await response.Content.ReadAsStringAsync());
            return obj.Property("key").Value.ToString();
        }

        public async Task ApproveSecretKeyAsync(string secretKey)
        {
            await client.PutAsync($"/api/keys/{secretKey}", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = 3,
                IsApproved = true,
            }), Encoding.UTF8, "application/json"));
        }

        public async Task RevokeSecretKeyAsync(string secretKey)
        {
            await client.PutAsync($"/api/keys/{secretKey}", new StringContent(JsonConvert.SerializeObject(new
            {
                KeyType = 3,
                IsApproved = false,
            }), Encoding.UTF8, "application/json"));
        }

        public async Task DeleteSecretKeyAsync(string secretKey)
        {
            await client.DeleteAsync($"/api/keys/{secretKey}");
        }
    }
}
