using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    [DataContract]
    public class ServiceMessage
    {
        [DataMember]
        public int Sender { get; set; }

        [DataMember]
        public DateTime DateTime { get; set; }
    }

    [DataContract]
    public class ServiceMessageText : ServiceMessage
    {
        [DataMember]
        public string Text { get; set; }
    }

    [DataContract]
    public class ServiceMessageFile : ServiceMessage
    {
        [DataMember]
        public Guid StreamId { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }
}
