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
    public class ConnectSecretKeyCommandTests
    {
        [TestMethod]
        public async Task When_secret_key_connect_with_valid_token_Then_connection_is_establish()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", "description", true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new ConnectSecretKeyCommand(id, token));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
            Assert.AreEqual(typeof(SecretKeyApproved), events[3].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[4]).Token);
            Assert.AreEqual(typeof(SecretKeyEaConnected), events[5].GetType());
        }

        [TestMethod]
        public async Task When_secret_key_connect_with_valid_token_two_times_Then_connection_is_establish()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", "description", true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new ConnectSecretKeyCommand(id, token));
            await mediator.Send(new ConnectSecretKeyCommand(id, token));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
            Assert.AreEqual(typeof(SecretKeyApproved), events[3].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[4]).Token);
            Assert.AreEqual(typeof(SecretKeyEaConnected), events[5].GetType());
        }

        [TestMethod]
        public async Task When_secret_key_connect_with_invalid_token_Then_connection_is_not_establish()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", "description", true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new ConnectSecretKeyCommand(id, "InvalidToken"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(5, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
            Assert.AreEqual(typeof(SecretKeyApproved), events[3].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[4]).Token);
        }

        [TestMethod]
        public async Task When_secret_key_connect_with_invalid_token_Given_no_token_generated_Then_connection_is_not_establish()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", "description", true));
            await mediator.Send(new ConnectSecretKeyCommand(id, "InvalidToken"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
            Assert.AreEqual(typeof(SecretKeyApproved), events[3].GetType());
        }

        [TestMethod]
        public async Task When_secret_key_connect_with_valid_token_Given_the_token_has_already_invalidated_Then_connection_is_not_establish()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", "description", true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Send(new InvalidateSecretTokenCommand(id, token));
            await mediator.Send(new ConnectSecretKeyCommand(id, token));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyDescriptionChanged), events[2].GetType());
            Assert.AreEqual("description", ((SecretKeyDescriptionChanged)events[2]).Description);
            Assert.AreEqual(typeof(SecretKeyApproved), events[3].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[4]).Token);
            Assert.AreEqual(typeof(SecretKeyTokenInvalidated), events[5].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenInvalidated)events[5]).Token);
        }
    }
}
