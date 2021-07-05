using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trsys.Web.Infrastructure.WriteModel.Tokens.InMemory;

namespace Trsys.Web.Infrastructure.Tests
{
    [TestClass]
    public class InMemoryTokenConnectionManagerStoreTests : TokenConnectionManagerStoreTestsBase
    {
        [TestInitialize]
        public void Setup()
        {
            sut = new InMemoryTokenConnectionManagerStore();
        }
    }
}
