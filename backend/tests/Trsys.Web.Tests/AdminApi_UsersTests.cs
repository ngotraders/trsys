using MediatR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
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
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            await mediator.Send(new UserCreateCommand("User Name 1", "user1", "email1@example.com", "PasswordHash", "Role"));
            await mediator.Send(new UserCreateCommand("User Name 2", "user2", "email2@example.com", "PasswordHash", "Role"));
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
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            await mediator.Send(new UserCreateCommand("User Name 1", "user1", "email1@example.com", "PasswordHash", "Role"));
            await mediator.Send(new UserCreateCommand("User Name 2", "user2", "email2@example.com", "PasswordHash", "Role"));
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

        [TestMethod]
        public async Task GetUser_should_return_ok_if_specified_key_exists()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            var id = await mediator.Send(new UserCreateCommand("User Name 1", "user1", "email1@example.com", "PasswordHash", "Role"));
            await client.LoginAsync();

            // Admin ユーザーが存在するので、1ページ目は2件
            var res = await client.GetAsync("/api/admin/users/" + id);
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var retObj = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(retObj.Contains("user1"));
        }

        [TestMethod]
        public async Task GetUser_should_return_not_found_if_specified_key_not_exists()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();
            var res = await client.GetAsync($"/api/admin/users/{Guid.NewGuid()}");
            Assert.AreEqual(HttpStatusCode.NotFound, res.StatusCode);
        }

        [TestMethod]
        public async Task PostUser_should_return_ok_if_valid_user_request()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();
            await client.LoginAsync();

            var res = await client.PostAsync("/api/admin/users", JsonContent.Create(new
            {
                Name = "User Name 1",
                Username = "user1",
                EmailAddress = "user1@example.com",
                Role = "Administrator",
                Password = "P@ssw0rd",
            }));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var obj = JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync());
            Assert.AreEqual("user1", obj.Property("username").Value.ToObject<string>());
            Assert.AreEqual("User Name 1", obj.Property("name").Value.ToObject<string>());
            Assert.AreEqual("user1@example.com", obj.Property("emailAddress").Value.ToObject<string>());
            Assert.AreEqual("Administrator", obj.Property("role").Value.ToObject<string>());
        }

        [TestMethod]
        public async Task PatchUser_should_return_ok_if_valid_request()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            var id = await mediator.Send(new UserCreateCommand("User Name 1", "user1", "email1@example.com", "PasswordHash", "Role"));
            await client.LoginAsync();

            var res = await client.PatchAsync($"/api/admin/users/{id}", JsonContent.Create(new
            {
                Name = "User Name 1 Updated",
                Username = "user1updated",
                EmailAddress = "user1updated@example.com",
                Role = "User",
                NewPassword = "P@ssw0rd",
            }));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);
            var obj = JsonConvert.DeserializeObject<JObject>(await res.Content.ReadAsStringAsync());
            Assert.AreEqual("User Name 1 Updated", obj.Property("name").Value.ToObject<string>());
            Assert.AreEqual("user1updated", obj.Property("username").Value.ToObject<string>());
            Assert.AreEqual("user1updated@example.com", obj.Property("emailAddress").Value.ToObject<string>());
            Assert.AreEqual("User", obj.Property("role").Value.ToObject<string>());
        }

        [TestMethod]
        public async Task PatchUser_should_return_bad_request_if_user_name_has_been_taken()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            var id = await mediator.Send(new UserCreateCommand("User Name 1", "user1", "email1@example.com", "PasswordHash", "Role"));
            await mediator.Send(new UserCreateCommand("User Name 2", "user2", "email2@example.com", "PasswordHash", "Role"));
            await client.LoginAsync();

            var res = await client.PatchAsync($"/api/admin/users/{id}", JsonContent.Create(new
            {
                Name = "User Name 1 Updated",
                Username = "user2",
                EmailAddress = "user1updated@example.com",
                Role = "User",
            }));
            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [TestMethod]
        public async Task DeleteUser_should_return_ok()
        {
            using var host = await TestHelper.CreateTestServerAsync();
            var server = host.GetTestServer();
            var client = server.CreateClient();

            var mediator = server.Services.GetService<IMediator>();
            var id = await mediator.Send(new UserCreateCommand("User Name 1", "user1", "email1@example.com", "PasswordHash", "Role"));
            await client.LoginAsync();

            var res = await client.DeleteAsync($"/api/admin/users/{id}");
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            // Data should be deleted
            res = await client.GetAsync($"/api/admin/users/{id}");
            Assert.AreEqual(HttpStatusCode.NotFound, res.StatusCode);
            res = await client.PatchAsync($"/api/admin/users/{id}", JsonContent.Create(new
            {
                Name = "User Name 1 Updated",
                Username = "user1updated",
                EmailAddress = "user1updated@example.com",
                Role = "User",
            }));
            Assert.AreEqual(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
