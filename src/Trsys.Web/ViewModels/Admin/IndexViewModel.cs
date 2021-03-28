using System.Collections.Generic;
using Trsys.Web.Models;

namespace Trsys.Web.ViewModels.Admin
{
    public class IndexViewModel
    {
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public SecretKeyType? KeyType { get; set; }

        public List<SecretKey> SecretKeys { get; set; }
    }
}
