using System;

namespace Trsys.Web.ViewModels.Home
{
    public class UserInfoViewModel
    {
        public Guid Id { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
