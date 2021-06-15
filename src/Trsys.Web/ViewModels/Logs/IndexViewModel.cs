using System.Collections.Generic;
using Trsys.Web.Models;

namespace Trsys.Web.ViewModels.Logs
{
    public class IndexViewModel
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public List<Log> Events { get; set; }

        public string Source { get; set; }
    }
}
