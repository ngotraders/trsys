using System;
using System.Collections.Generic;
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

        public async Task CreateKeyAsync(string secretKey = default)
        {
            await client.PostAsync("/admin/keys/new", new FormUrlEncodedContent(
                new KeyValuePair<string, string>[] {
                    KeyValuePair.Create("Key", secretKey),
                    KeyValuePair.Create("KeyType", "3"),
                }));
        }

        public async Task ApproveSecretKeyAsync(string secretKey)
        {
            await client.PostAsync($"/admin/keys/{secretKey}/approve", new StringContent("", Encoding.UTF8, "text/plain"));
        }

        public async Task RevokeSecretKeyAsync(string secretKey)
        {
            await client.PostAsync($"/admin/keys/{secretKey}/revoke", new StringContent("", Encoding.UTF8, "text/plain"));
        }

        public async Task DeleteSecretKeyAsync(string secretKey)
        {
            await client.PostAsync($"/admin/keys/{secretKey}/delete", new StringContent("", Encoding.UTF8, "text/plain"));
        }
    }
}
