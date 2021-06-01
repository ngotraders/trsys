using System;

namespace Trsys.Web.Models.ReadModel.Dtos
{
    public class OrderDto
    {
        public string Id { get; set; }
        public Guid SecretKeyId { get; set; }
        public PublishedOrder Order { get; set; }
    }
}
