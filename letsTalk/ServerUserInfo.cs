using System.Runtime.Serialization;
using System.ServiceModel;

namespace letsTalk
{
    // DataContract -> данный объект будет сериализоваться
    // DataMember -> поле должно сериализоваться

    // IsRequired -> обязательно отправляет/получает поле
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
}