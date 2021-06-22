using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory;

namespace Trsys.Web.Infrastructure.Tests
{
    [TestClass]
    public class InMemoryTokenConnectionManagerStoreTests : TokenConnectionManagerStoreTestsBase
    {
        [TestInitialize]
        public async Task Setup()
        {
            sut = new InMemoryTokenConnectionManagerStore();
            await sut.TryRemoveAsync("Token");
        }
        [TestCleanup]
        public async Task Teardown()
        {
            await sut.TryRemoveAsync("Token");
        }
    }
}
