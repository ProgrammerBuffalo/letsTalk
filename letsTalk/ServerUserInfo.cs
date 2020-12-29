using System.Runtime.Serialization;
using System.ServiceModel;

namespace letsTalk
{

    [DataContract]
    public class AuthenticationUserInfo
    {
        [DataMember(IsRequired = true)]
        public string Login { get; set; }

        [DataMember(IsRequired = true)]
        public string Password { get; set; }
    }

    [DataContract]
    public class ServerUserInfo : AuthenticationUserInfo
    {
        [DataMember(IsRequired = false)]
        public int SqlId { get; set; }

        [DataMember(IsRequired = true)]
        public string Name { get; set; }

    }
    
    [DataContract]
    public class ConnectedServerUser
    {
        [DataMember]
        public int SqlId { get; set; }

        [DataMember]
        public OperationContext OperationContext { get; set; }

    }

}