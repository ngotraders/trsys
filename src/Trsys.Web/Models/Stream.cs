using System.Collections.Generic;

namespace Trsys.Models
{
    public partial class Stream
    {
        public Stream()
        {
            Messages = new HashSet<Message>();
        }

        public string Id { get; set; }
        public string IdOriginal { get; set; }
        public int IdInternal { get; set; }
        public int Version { get; set; }
        public long Position { get; set; }
        public int? MaxAge { get; set; }
        public int? MaxCount { get; set; }
        public string IdOriginalReversed { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}
