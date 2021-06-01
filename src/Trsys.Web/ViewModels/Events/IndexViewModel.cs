using System.Collections.Generic;
using Trsys.Web.Models;
using Trsys.Web.Models.ReadModel.Dtos;

namespace Trsys.Web.ViewModels.Events
{
    public class IndexViewModel
    {
        public int Page { get; set; }
        public int PerPage { get; set; }
        public List<SecretKeyDto> SecretKeys { get; set; }
        public List<Event> Events { get; set; }

        public string Source { get; set; }
    }
}
