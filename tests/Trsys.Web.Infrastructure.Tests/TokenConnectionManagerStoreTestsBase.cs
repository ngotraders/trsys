using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.WriteModel.Tokens;
using Trsys.Web.Infrastructure.WriteModel.Tokens.Redis;

namespace Trsys.Web.Infrastructure.Tests
{
    public class TokenConnectionManagerStoreTestsBase
    {
        protected ITokenConnectionManagerStore sut;

        [TestMethod]
        public async Task When_add_token_Then_token_in_use_returns_false()
        {
            Assert.IsTrue(await sut.TryAddAsync("Token", Guid.NewGuid()));
            Assert.IsFalse(await sut.IsTokenInUseAsync("Token"));
        }

        [TestMethod]
        public async Task When_add_same_token_twice_Then_token_in_use_returns_false()
        {
            Assert.IsTrue(await sut.TryAddAsync("Token", Guid.NewGuid()));
            Assert.IsFalse(await sut.TryAddAsync("Token", Guid.NewGuid()));
            Assert.IsFalse(await sut.IsTokenInUseAsync("Token"));
        }

        [TestMethod]
        public async Task When_touch_token_for_the_first_time_Then_returns_true()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            var result = await sut.ExtendTokenExpirationTimeAsync("Token");
            Assert.IsTrue(result.Item1);
            Assert.AreEqual(id, result.Item2);
        }

        [TestMethod]
        public async Task When_touch_token_for_the_first_time_and_check_token_usage_Then_returns_true()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            var result = await sut.ExtendTokenExpirationTimeAsync("Token");
            Assert.IsTrue(result.Item1);
            Assert.AreEqual(id, result.Item2);
            Assert.IsTrue(await sut.IsTokenInUseAsync("Token"));
        }

        [TestMethod]
        public async Task Given_token_touched_When_touch_token_Then_returns_false()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            var result = await sut.ExtendTokenExpirationTimeAsync("Token");
            Assert.IsFalse(result.Item1);
            if (sut is RedisTokenConnectionManagerStore)
            {
                Assert.AreEqual(Guid.Empty, result.Item2);
            }
            else
            {
                Assert.AreEqual(id, result.Item2);
            }
            Assert.IsTrue(await sut.IsTokenInUseAsync("Token"));
        }

        [TestMethod]
        public async Task Given_token_touched_and_wait_for_a_second_Then_returns_false_and_id()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            await Task.Delay(1000);
            var result = await sut.ExtendTokenExpirationTimeAsync("Token");
            Assert.IsFalse(result.Item1);
            Assert.AreEqual(id, result.Item2);
            Assert.IsTrue(await sut.IsTokenInUseAsync("Token"));
        }

        [TestMethod]
        public async Task Given_token_is_not_connected_When_searching_connecting_token_Then_token_is_not_retrieved_as_connected()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            var connections = await sut.SearchConnectionsAsync();
            Assert.IsFalse(connections.Any(c => c.Item1 == "Token"));
        }

        [TestMethod]
        public async Task When_searching_connecting_token_Then_token_is_retrieved_as_connected()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            var connections = await sut.SearchConnectionsAsync();
            Assert.IsTrue(connections.Any(c => c.Item1 == "Token"));
        }

        [TestMethod]
        public async Task When_clearing_token_connection_Then_token_is_not_retrieved_as_connected()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            await sut.ClearExpirationTimeAsync("Token");
            var connections = await sut.SearchConnectionsAsync();
            Assert.IsFalse(connections.Any(c => c.Item1 == "Token"));
        }

        [TestMethod]
        public async Task Given_token_is_not_expired_When_search_expired_token_Then_token_is_not_retrieved_as_expired()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            var connections = await sut.SearchExpiredTokensAsync();
            Assert.IsFalse(connections.Contains("Token"));
        }

        [TestMethod]
        public async Task Given_token_is_expired_When_search_expired_token_Then_token_is_retrieved_as_expired()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            await Task.Delay(5000);
            var connections = await sut.SearchExpiredTokensAsync();
            Assert.IsTrue(connections.Contains("Token"));
        }

        [TestMethod]
        public async Task Given_token_is_not_connected_When_search_expired_token_Then_token_is_not_retrieved_as_expired()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            var connections = await sut.SearchExpiredTokensAsync();
            Assert.IsFalse(connections.Contains("Token"));
        }

        [TestMethod]
        public async Task Given_token_is_not_connected_When_delete_token_Then_returns_false()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            var result = await sut.TryRemoveAsync("Token");
            Assert.IsFalse(result.Item1);
            Assert.AreEqual(id, result.Item2);
        }

        [TestMethod]
        public async Task Given_token_is_connected_When_delete_token_Then_returns_false()
        {
            var id = Guid.NewGuid();
            Assert.IsTrue(await sut.TryAddAsync("Token", id));
            await sut.ExtendTokenExpirationTimeAsync("Token");
            var result = await sut.TryRemoveAsync("Token");
            Assert.IsTrue(result.Item1);
            Assert.AreEqual(id, result.Item2);
        }
    }
}
