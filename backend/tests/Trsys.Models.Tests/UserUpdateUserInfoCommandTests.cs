using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure;
using Trsys.Models.Events;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Models.Tests
{
    [TestClass]
    public class UserUpdateUserInfoCommandTests
    {
        [TestMethod]
        public async Task When_update_userinfo_then_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new UserCreateCommand("name", "username", "pass", "Administrator"));
            await mediator.Send(new UserUpdateUserInfoCommand(id, "name2", "email@example.com"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(UserCreated), events[0].GetType());
            Assert.AreEqual("name", ((UserCreated)events[0]).Name);
            Assert.AreEqual("username", ((UserCreated)events[0]).Username);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[1].GetType());
            Assert.AreEqual("pass", ((UserPasswordHashChanged)events[1]).PasswordHash);
            Assert.AreEqual(typeof(UserUserInfoUpdated), events[2].GetType());
            Assert.AreEqual("name2", ((UserUserInfoUpdated)events[2]).Name);
            Assert.AreEqual("email@example.com", ((UserUserInfoUpdated)events[2]).EmailAddress);
        }
        [TestMethod]
        public async Task When_update_userinfo_samevalue_then_nothing_occurs()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new UserCreateCommand("name", "username", "pass", "Administrator"));
            await mediator.Send(new UserUpdateUserInfoCommand(id, "name2", "email@example.com"));
            await mediator.Send(new UserUpdateUserInfoCommand(id, "name2", "email@example.com"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(UserCreated), events[0].GetType());
            Assert.AreEqual("name", ((UserCreated)events[0]).Name);
            Assert.AreEqual("username", ((UserCreated)events[0]).Username);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[1].GetType());
            Assert.AreEqual("pass", ((UserPasswordHashChanged)events[1]).PasswordHash);
            Assert.AreEqual(typeof(UserUserInfoUpdated), events[2].GetType());
            Assert.AreEqual("name2", ((UserUserInfoUpdated)events[2]).Name);
            Assert.AreEqual("email@example.com", ((UserUserInfoUpdated)events[2]).EmailAddress);
        }
    }
}
