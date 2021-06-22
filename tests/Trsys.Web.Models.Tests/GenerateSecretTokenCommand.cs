using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Web.Infrastructure;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.WriteModel.Commands;
using Trsys.Web.Models.WriteModel.Infrastructure;
using Trsys.Web.Models.WriteModel.Notifications;

namespace Trsys.Web.Models.Tests
{
    [TestClass]
    public class GenerateSecretTokenCommandTests
    {
        [TestMethod]
        public async Task Given_token_is_empty_Then_generates_token()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);
        }

        [TestMethod]
        public async Task Given_token_is_not_empty_Then_invalidates_old_token_and_generates_new_token()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            var token2 = await mediator.Send(new GenerateSecretTokenCommand(id));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(6, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);
            Assert.AreEqual(typeof(SecretKeyTokenInvalidated), events[4].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenInvalidated)events[4]).Token);
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[5].GetType());
            Assert.AreEqual(token2, ((SecretKeyTokenGenerated)events[5]).Token);
        }

        [TestMethod]
        public async Task Given_token_is_in_use_Then_fail_generation()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, true));
            var token = await mediator.Send(new GenerateSecretTokenCommand(id));
            await mediator.Publish(new TokenTouched(token));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new GenerateSecretTokenCommand(id)));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(4, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
            Assert.AreEqual(typeof(SecretKeyApproved), events[2].GetType());
            Assert.AreEqual(typeof(SecretKeyTokenGenerated), events[3].GetType());
            Assert.AreEqual(token, ((SecretKeyTokenGenerated)events[3]).Token);

            var connectionStore = services.GetRequiredService<ITokenConnectionManager>();
            Assert.IsTrue(await connectionStore.IsTokenInUseAsync(token));
        }

        [TestMethod]
        public async Task Given_key_is_not_approved_Then_fail_generation()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateSecretKeyCommand(SecretKeyType.Publisher, "KEY", null, null));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new GenerateSecretTokenCommand(id)));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(SecretKeyCreated), events[0].GetType());
            Assert.AreEqual("KEY", ((SecretKeyCreated)events[0]).Key);
            Assert.AreEqual(typeof(SecretKeyKeyTypeChanged), events[1].GetType());
            Assert.AreEqual(SecretKeyType.Publisher, ((SecretKeyKeyTypeChanged)events[1]).KeyType);
        }
    }
}
