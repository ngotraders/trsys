using System;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class SecretKeyDto
    {
        public Guid Id { get; set; }
        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool IsValid { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }
        public bool IsConnected => LastConnected.HasValue ? DateTimeOffset.UtcNow - LastConnected.Value < TimeSpan.FromSeconds(5) : false;
        public DateTimeOffset? LastConnected { get; set; }
    }
}
