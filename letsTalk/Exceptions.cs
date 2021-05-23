using System.Runtime.Serialization;

namespace letsTalk
{
    // !!! Возможные ошибки, которые следует сообщать клиенту
    
    // DataContract -> данный объект будет сериализоваться
    // DataMember -> поле должно сериализоваться
 
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
    public class AddChatExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public AddChatExceptionFault()
        {
            Message = "The user has not confirmed his exit";
        }
    }

    [DataContract]
    public class ChatroomAlreadyExistExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public ChatroomAlreadyExistExceptionFault()
        {
            Message = "Chatroom already exsists, cannot create a new one";
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

    [DataContract]
    public class StreamExceptionFault
    {
        [DataMember]
        public string Message { get; private set; }

        public StreamExceptionFault()
        {
            Message = "Failed to send file";
        }
    }
}
