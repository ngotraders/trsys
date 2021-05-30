using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.ReadModel.Queries;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class CreateSecretKeyCommandTests
    {
        [TestMethod]
        public async Task CreateSecretKeyCommandTests1()
        {
            using var services = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            await mediator.Send(new CreateSecretKeyCommand(null, "TEST_KEY", null));
            var list = await mediator.Send(new GetSecretKeys());
            Assert.AreEqual(1, list.Count);
        }
    }
}
