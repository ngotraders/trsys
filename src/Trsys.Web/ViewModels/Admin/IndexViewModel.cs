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
    }
}
