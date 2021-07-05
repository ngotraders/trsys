using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.WriteModel.Tokens;

namespace Trsys.Web.Infrastructure.Tests
{
    public class TokenConnectionManagerStoreTestsBase
    {
        protected ISecretKeyConnectionManagerStore sut;

        [TestMethod]
        public async Task Given_by_default_Then_is_connected_returns_false()
        {
            var id = Guid.NewGuid();
            Assert.IsFalse(await sut.IsConnectedAsync(id));
        }

        [TestMethod]
        public async Task When_touch_secret_key_id_for_the_first_time_Then_returns_true()
        {
            var id = Guid.NewGuid();
            var result = await sut.UpdateLastAccessedAsync(id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task When_touch_secret_key_id_for_the_first_time_and_check_secret_key_id_usage_Then_returns_true()
        {
            var id = Guid.NewGuid();
            var result = await sut.UpdateLastAccessedAsync(id);
            Assert.IsTrue(result);
            Assert.IsTrue(await sut.IsConnectedAsync(id));
        }

        [TestMethod]
        public async Task Given_secret_key_id_touched_When_touch_the_id_Then_returns_false()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            var result = await sut.UpdateLastAccessedAsync(id);
            Assert.IsFalse(result);
            Assert.IsTrue(await sut.IsConnectedAsync(id));
        }

        [TestMethod]
        public async Task Given_secret_key_id_touched_and_wait_for_a_second_Then_is_connected_returns_false()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            await Task.Delay(1000);
            var result = await sut.UpdateLastAccessedAsync(id);
            Assert.IsFalse(result);
            Assert.IsTrue(await sut.IsConnectedAsync(id));
        }

        [TestMethod]
        public async Task Given_secret_key_id_is_not_connected_When_searching_for_connected_ids_Then_retrieves_any_ids()
        {
            var connections = await sut.SearchConnectedSecretKeysAsync();
            Assert.IsFalse(connections.Any());
        }

        [TestMethod]
        public async Task Given_secret_key_id_is_connected_When_searching_for_connected_ids_Then_retrieves_as_connected()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            var connections = await sut.SearchConnectedSecretKeysAsync();
            Assert.IsTrue(connections.Any(c => c == id));
        }

        [TestMethod]
        public async Task When_clearing_secret_key_id_connection_Then_secret_key_id_is_not_retrieved_as_connected()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            await sut.ClearConnectionAsync(id);
            var connections = await sut.SearchConnectedSecretKeysAsync();
            Assert.IsFalse(connections.Any());
        }

        [TestMethod]
        public async Task Given_secret_key_id_is_not_expired_When_search_expired_ids_Then_the_id_is_not_retrieved_as_expired()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            var connections = await sut.SearchExpiredSecretKeysAsync();
            Assert.IsFalse(connections.Contains(id));
        }

        [TestMethod]
        public async Task Given_secret_key_id_is_expired_When_search_expired_ids_Then_the_id_is_retrieved_as_expired()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            await Task.Delay(5000);
            var connections = await sut.SearchExpiredSecretKeysAsync();
            Assert.IsTrue(connections.Contains(id));
        }

        [TestMethod]
        public async Task Given_secret_key_id_is_not_connected_When_search_expired_ids_Then_any_ids_are_retrieved()
        {
            var connections = await sut.SearchExpiredSecretKeysAsync();
            Assert.IsFalse(connections.Any());
        }

        [TestMethod]
        public async Task Given_secret_key_id_is_connected_When_clear_secret_key_id_Then_returns_true()
        {
            var id = Guid.NewGuid();
            await sut.UpdateLastAccessedAsync(id);
            var result = await sut.ClearConnectionAsync(id);
            Assert.IsTrue(result);
        }
    }
}
