using System;
using System.Collections.Generic;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.ViewModels.Admin
{
    public class IndexViewModel
    {
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public SecretKeyType? KeyType { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }

        public List<SecretKeyDto> SecretKeys { get; set; }
        public int SecretKeysTotalCount { get; set; }
        public int SecretKeysPage { get; set; }
        public int SecretKeysPerPage { get; set; }
        public string CacheOrderText { get; set; }
        public string EaSiteUrl { get; set; }

        public string NewOrderSecretKey { get; set; }
        public int? NewOrderTicketNo { get; set; }
        public string NewOrderSymbol { get; set; }
        public OrderType? NewOrderType { get; set; }
        public DateTimeOffset? NewOrderTime { get; set; }
        public decimal? NewOrderPrice { get; set; }
        public decimal? NewOrderPercentage { get; set; }

        public string CloseOrderSecretKey { get; set; }
        public int? CloseOrderTicketNo { get; set; }
    }
}
