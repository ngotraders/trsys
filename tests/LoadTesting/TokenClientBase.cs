using NBomber.Contracts;
using Serilog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTesting
{
    public abstract class TokenClientBase
    {
        protected string SecretKey { get; }
        protected string EaType { get; }
        protected HttpClient Client { get; }

        private string secretToken;
        private bool isInit = false;
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);

        public TokenClientBase(string endpoint, string secretKey, string eaType)
        {
            SecretKey = secretKey;
            EaType = eaType;
            Client = HttpClientFactory.Create(endpoint, true);
        }

        public async Task InitializeAsync()
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Ea-Id", SecretKey);
            Client.DefaultRequestHeaders.Add("X-Ea-Type", EaType);
            Client.DefaultRequestHeaders.Add("X-Ea-Version", "20211109");
            var res = await Client.PostAsync("/api/ea/token/generate", new StringContent(SecretKey, Encoding.UTF8, "text/plain"));
            res.EnsureSuccessStatusCode();
            secretToken = await res.Content.ReadAsStringAsync();

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Ea-Id", SecretKey);
            Client.DefaultRequestHeaders.Add("X-Ea-Type", EaType);
            Client.DefaultRequestHeaders.Add("X-Ea-Version", "20211109");
            Client.DefaultRequestHeaders.Add("X-Secret-Token", secretToken);
            isInit = true;
        }

        public async Task FinalizeAsync()
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("X-Ea-Id", SecretKey);
            Client.DefaultRequestHeaders.Add("X-Ea-Type", EaType);
            Client.DefaultRequestHeaders.Add("X-Ea-Version", "20211109");
            var res = await Client.PostAsync("/api/ea/token/" + Uri.UnescapeDataString(secretToken) + "/release", new StringContent(SecretKey, Encoding.UTF8, "text/plain"));
            res.EnsureSuccessStatusCode();
            secretToken = null;
            Client.DefaultRequestHeaders.Clear();
            isInit = false;
        }

        public async Task<IResponse> ExecuteAsync()
        {
            EnsureInited();

            await lockObject.WaitAsync();
            try
            {
                return await OnExecuteAsync();
            }
            finally
            {
                lockObject.Release();
            }
        }

        private void EnsureInited()
        {
            if (!isInit) throw new InvalidOperationException("not initialized");
        }

        protected abstract Task<IResponse> OnExecuteAsync();
    }
}
