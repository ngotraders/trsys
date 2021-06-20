using System;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class SecretKeyDto
    {
        public Guid Id { get; set; }
        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool IsApproved { get; set; }
        public string Token { get; set; }
        public bool IsConnected { get; set; }
    }
}
