using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.ReadModel.Events;
using Trsys.Web.Models.WriteModel.Commands;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class CreateSecretKeyCommandTests
    {
        [TestMethod]
        public async Task When_KeyType_specified_then_secret_key_created_successfully()
        {
            using var services = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "TEST_KEY", "description"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("TEST_KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
        }

        [TestMethod]
        public async Task When_KeyType_Key_and_Description_is_not_specified_then_secret_key_created_successfully()
        {
            using var services = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(null, null, null));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.IsNotNull(((SecretKeyCreated)events[0]).Key);
        }

        [TestMethod]
        public async Task Given_Key_already_exists_Then_no_event_created()
        {
            using var services = new ServiceCollection().AddInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(null, "TEST_KEY", null));
            Assert.AreEqual(id, await mediator.Send(new CreateSecretKeyCommand(null, "TEST_KEY", null)));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("TEST_KEY", ((SecretKeyCreated)events[0]).Key);
        }
    }
}
