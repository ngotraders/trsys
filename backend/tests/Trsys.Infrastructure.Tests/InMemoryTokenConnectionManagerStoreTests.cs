using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trsys.Infrastructure.WriteModel.Tokens.InMemory;

namespace Trsys.Infrastructure.Tests
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
