using System.Runtime.Serialization;

namespace letsTalk
{
    [DataContract]
    public class LoginExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public LoginExceptionFault()
        {
            Message = "This login already exists";
        }
    }

    [DataContract]
    public class NicknameExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public NicknameExceptionFault()
        {
            Message = "This name already exists";
        }
    }

    [DataContract]
    public class AuthorizationExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public AuthorizationExceptionFault()
        {
            Message = "Login or Password is not correct";
        }
    }

    [DataContract]
    public class ConnectionExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public ConnectionExceptionFault()
        {
            Message = "This user is already connected";
        }
    }
}
