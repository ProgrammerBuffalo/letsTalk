using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    [DataContract(Name = "RulingMessage")]
    public enum RulingMessage
    {
        [EnumMember] UserJoined = 1,
        [EnumMember] UserLeft = 2,
        [EnumMember] UserRemoved = 3,
        [EnumMember] ChatroomDelete = 4
    }

    [DataContract]
    [KnownType(typeof(ServiceMessageText))]
    [KnownType(typeof(ServiceMessageFile))]
    [KnownType(typeof(ServiceMessageManage))]
    public class ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public DateTime DateTime { get; set; }
    }

    [DataContract]
    public class ServiceMessageText : ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public string Text { get; set; }

        [DataMember(IsRequired = true)]
        public int UserId { get; set; }
    }

    [DataContract]
    public class ServiceMessageFile : ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public Guid StreamId { get; set; }

        [DataMember(IsRequired = true)]
        public string FileName { get; set; }

        [DataMember(IsRequired = true)]
        public int UserId { get; set; }
    }

    [DataContract]
    public class ServiceMessageManage : ServiceMessage
    {
        [DataMember(IsRequired = true)]
        public string UserNickname { get; set; }

        [DataMember(IsRequired = true)]
        public RulingMessage RulingMessage { get; set; }
    }

}