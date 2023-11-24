using CQRSlite.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trsys.Infrastructure;
using Trsys.Models.Events;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Models.Tests
{
    [TestClass]
    public class CreateUserCommandTests
    {
        [TestMethod]
        public async Task When_creates_first_time_Then_succeeds()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateUserCommand("name", "username", "pass", "Administrator"));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(UserCreated), events[0].GetType());
            Assert.AreEqual("name", ((UserCreated)events[0]).Name);
            Assert.AreEqual("username", ((UserCreated)events[0]).Username);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[1].GetType());
            Assert.AreEqual("pass", ((UserPasswordHashChanged)events[1]).PasswordHash);
        }

        [TestMethod]
        public async Task When_creates_second_time_Then_throws_error()
        {
            using var services = new ServiceCollection().AddInMemoryInfrastructure().BuildServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var id = await mediator.Send(new CreateUserCommand("name", "username", "pass", "Administrator"));
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await mediator.Send(new CreateUserCommand("name", "username", "pass", "Administrator")));

            var store = services.GetRequiredService<IEventStore>();
            var events = (await store.Get(id, 0)).ToList();

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(typeof(UserCreated), events[0].GetType());
            Assert.AreEqual("name", ((UserCreated)events[0]).Name);
            Assert.AreEqual("username", ((UserCreated)events[0]).Username);
            Assert.AreEqual(typeof(UserPasswordHashChanged), events[1].GetType());
            Assert.AreEqual("pass", ((UserPasswordHashChanged)events[1]).PasswordHash);
        }
    }
}
