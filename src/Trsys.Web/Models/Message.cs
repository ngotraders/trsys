using System;

namespace Trsys.Models
{
    public partial class Message
    {
        public int StreamIdInternal { get; set; }
        public int StreamVersion { get; set; }
        public long Position { get; set; }
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }
        public string JsonData { get; set; }
        public string JsonMetadata { get; set; }

        public virtual Stream StreamIdInternalNavigation { get; set; }
    }
}
