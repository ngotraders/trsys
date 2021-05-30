using CQRSlite.Commands;
using CQRSlite.Queries;
using CQRSlite.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.ReadModel.Handlers;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Handlers;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class CreateSecretKeyCommandTests
    {
        [TestMethod]
        public async Task CreateSecretKeyCommandTests1()
        {
            using var services = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
            var registrar = new RouteRegistrar(services);
            registrar.RegisterHandlers(typeof(SecretKeyCommandHandlers));
            registrar.RegisterHandlers(typeof(SecretKeyListView));

            var sut = services.GetRequiredService<ICommandSender>();
            await sut.Send(new CreateSecretKeyCommand(null, "TEST_KEY", null));
            var query = services.GetRequiredService<IQueryProcessor>();
            var list = await query.Query(new GetSecretKeys());
            Assert.AreEqual(1, list.Count);
        }
    }
}
