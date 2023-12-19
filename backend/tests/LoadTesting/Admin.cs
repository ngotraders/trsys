using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LoadTesting
{
    public class SecretKey
    {
        public string Id { get; set; }
        public string Key { get; set; }
    }
    public class Admin
    {
        private readonly HttpClient client;
        private readonly string email;
        private readonly string password;

        public Admin(string endpointUrl, string email, string password)
        {
            client = HttpClientFactory.Create(endpointUrl, true);
            client.BaseAddress = new Uri(endpointUrl);
            this.email = email;
            this.password = password;
        }

        public async Task LoginAsync()
        {
            await client.PostAsync("/login?useCookies=true", JsonContent.Create(
                new
                {
                    Email = email,
                    Password = password,
                }));
        }

        public async Task<List<SecretKey>> GetSecretKeysAsync()
        {
            var response = await client.GetAsync("/api/admin/secret-keys");
            return JsonConvert.DeserializeObject<List<SecretKey>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<SecretKey> CreateKeyAsync(string secretKey = default)
        {
            var response = await client.PostAsync("/api/admin/secret-keys", JsonContent.Create(new
            {
                Key = secretKey,
                KeyType = 3,
            }));
            return JsonConvert.DeserializeObject<SecretKey>(await response.Content.ReadAsStringAsync());
        }

        public async Task ApproveSecretKeyAsync(string secretKeyId)
        {
            await client.PatchAsync($"/api/admin/secret-keys/{secretKeyId}", JsonContent.Create(new
            {
                KeyType = 3,
                IsApproved = true,
            }));
        }

        public async Task RevokeSecretKeyAsync(string secretKeyId)
        {
            await client.PatchAsync($"/api/admin/secret-keys/{secretKeyId}", JsonContent.Create(new
            {
                KeyType = 3,
                IsApproved = false,
            }));
        }

        public async Task DeleteSecretKeyAsync(string secretKeyId)
        {
            await client.DeleteAsync($"/api/admin/secret-keys/{secretKeyId}");
        }
    }
}
