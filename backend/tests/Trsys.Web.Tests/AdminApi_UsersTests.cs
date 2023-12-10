using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Trsys.Models.WriteModel.Commands;

namespace Trsys.Web.Tests
{
    [TestClass]
    public class AdminApi_UsersTests
    {
        [TestMethod]
        public async Task Index_search_users()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            await mediator.Send(new UserCreateCommand("user1", "User Name 1", "PasswordHash", "Role"));
            await mediator.Send(new UserCreateCommand("user2", "User Name 2", "PasswordHash", "Role"));
            await client.LoginAsync();

            var res = await client.GetAsync("/api/admin/users");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(3, int.Parse(res.Headers.GetValues("X-Total-Count").First()));
            var retObj = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(retObj.Contains("user1"));
            Assert.IsTrue(retObj.Contains("user2"));
        }

        [TestMethod]
        public async Task Index_search_users_with_pagenation()
        {
            var server = TestHelper.CreateServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            await mediator.Send(new UserCreateCommand("user1", "User Name 1", "PasswordHash", "Role"));
            await mediator.Send(new UserCreateCommand("user2", "User Name 2", "PasswordHash", "Role"));
            await client.LoginAsync();

            // Admin ユーザーが存在するので、1ページ目は2件
            var res = await client.GetAsync("/api/admin/users?_start=0&_end=2");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(3, int.Parse(res.Headers.GetValues("X-Total-Count").First()));
            var retObj = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(retObj.Contains("admin"));
            Assert.IsTrue(retObj.Contains("user1"));
            Assert.IsFalse(retObj.Contains("user2"));

            res = await client.GetAsync("/api/admin/users?_start=2&_end=4");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            Assert.AreEqual(3, int.Parse(res.Headers.GetValues("X-Total-Count").First()));
            retObj = await res.Content.ReadAsStringAsync();
            Assert.IsFalse(retObj.Contains("admin"));
            Assert.IsFalse(retObj.Contains("user1"));
            Assert.IsTrue(retObj.Contains("user2"));
        }
    }
}