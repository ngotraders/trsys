using System.Collections.Generic;

namespace Trsys.Infrastructure.ReadModel.UserNotification
{
    public class EmailSenderConfiguration
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public bool? UseSsl { get; set; }
        public string AuthenticationUser { get; set; }
        public string AuthenticationPassword { get; set; }
        public string MailFrom { get; set; }
        public string AuthenticationClientId { get; set; }
        public string AuthenticationAuthority { get; set; }
        public string AuthenticationClientSecret { get; set; }
        public List<string> AuthenticationScopes { get; set; }
    }
}