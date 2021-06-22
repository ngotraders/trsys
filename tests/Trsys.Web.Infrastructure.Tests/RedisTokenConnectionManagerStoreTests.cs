using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.WriteModel.Tokens.Redis;

namespace Trsys.Web.Infrastructure.Tests
{
    [TestClass]
    public class RedisTokenConnectionManagerStoreTests : TokenConnectionManagerStoreTestsBase
    {
        private ConnectionMultiplexer connection;

        [TestInitialize]
        public async Task Setup()
        {
            connection = await ConnectionMultiplexer.ConnectAsync("127.0.0.1");
            sut = new RedisTokenConnectionManagerStore(connection);
            await sut.TryRemoveAsync("Token");
        }
        [TestCleanup]
        public async Task Teardown()
        {
            await sut.TryRemoveAsync("Token");
            connection.Dispose();
        }
    }
}
