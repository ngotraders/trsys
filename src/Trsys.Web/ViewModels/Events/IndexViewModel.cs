using System.Collections.Generic;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.SecretKeys;

namespace Trsys.Web.ViewModels.Events
{
    public class IndexViewModel
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public List<SecretKey> SecretKeys { get; set; }
        public List<Event> Events { get; set; }

        public string Source { get; set; }
    }
}
