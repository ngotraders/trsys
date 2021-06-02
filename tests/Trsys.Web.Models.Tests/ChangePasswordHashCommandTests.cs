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
    public class ChangePasswordHashCommandTests
    {
        [TestMethod]
        public async Task When_changing_with_same_password_Then_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateUserCommand("user", "username", "password", "Administrator"));
            await mediator.Send(new ChangePasswordHashCommand(id, "password"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(UserCreated), events[0].GetType());
            Assert.AreEqual("user", ((UserCreated)events[0]).Name);
            Assert.AreEqual("username", ((UserCreated)events[0]).Username);
            Assert.AreEqual("Administrator", ((UserCreated)events[0]).Role);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[1].GetType());
            Assert.AreEqual("password", ((UserPasswordHashChanged)events[1]).PasswordHash);
        }
        [TestMethod]
        public async Task When_changing_with_different_password_Then_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateUserCommand("user", "username", "password", "Administrator"));
            await mediator.Send(new ChangePasswordHashCommand(id, "newPassword"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(typeof(UserCreated), events[0].GetType());
            Assert.AreEqual("user", ((UserCreated)events[0]).Name);
            Assert.AreEqual("username", ((UserCreated)events[0]).Username);
            Assert.AreEqual("Administrator", ((UserCreated)events[0]).Role);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[1].GetType());
            Assert.AreEqual("password", ((UserPasswordHashChanged)events[1]).PasswordHash);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[2].GetType());
            Assert.AreEqual("newPassword", ((UserPasswordHashChanged)events[2]).PasswordHash);
        }
    }
}
