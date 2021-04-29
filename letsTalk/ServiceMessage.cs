using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    [DataContract]
    [KnownType(typeof(ServiceMessageText))]
    [KnownType(typeof(ServiceMessageFile))]
    public class ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public int Sender { get; set; }

        [DataMember(IsRequired = true)]
        public DateTime DateTime { get; set; }
    }

    [DataContract]
    public class ServiceMessageText : ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public string Text { get; set; }
    }

    [DataContract]
    public class ServiceMessageFile : ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public Guid StreamId { get; set; }

        [DataMember(IsRequired = true)]
        public string FileName { get; set; }
    }
}
