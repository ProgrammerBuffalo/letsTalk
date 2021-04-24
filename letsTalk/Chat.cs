using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    [DataContract]
    public class Chatroom
    {
        [DataMember]
        public int ChatSqlId { get; set; }

        [DataMember]
        public string ChatName { get; set; }
    }

    public class UserInChat
    {
        [DataMember]
        public int UserSqlId { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }
    }
}
