using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Threading.Tasks;
using Trsys.Infrastructure.WriteModel.Tokens.Redis;

namespace Trsys.Infrastructure.Tests
{
    [TestClass]
    [Ignore]
    public class RedisTokenConnectionManagerStoreTests : TokenConnectionManagerStoreTestsBase
    {
        private ConnectionMultiplexer connection;

        [TestInitialize]
        public async Task Setup()
        {
            connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            sut = new RedisTokenConnectionManagerStore(connection);
            foreach (var connection in await sut.SearchConnectedSecretKeysAsync())
            {
                await sut.ClearConnectionAsync(connection.Id);
            }
        }
        [TestCleanup]
        public async Task Teardown()
        {
            foreach (var connection in await sut.SearchConnectedSecretKeysAsync())
            {
                await sut.ClearConnectionAsync(connection.Id);
            }
            connection.Dispose();
        }
    }
}
