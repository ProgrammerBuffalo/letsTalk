using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace letsTalk
{
    [ServiceContract(Name = "Chat", Namespace = "letsTalk.IChatService")]
    public interface IChatService
    {
        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(LoginExceptionFault))]
        [FaultContract(typeof(NicknameExceptionFault))]
        int Registration(ServerUserInfo serverUserInfo);

        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(AuthorizationExceptionFault))]
        ServerUserInfo Authorization(AuthenticationUserInfo authenticationUserInfo);

        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(ConnectionExceptionFault))]
        Guid Connect(int sqlId);

        [OperationContract]
        void Disconnect(Guid UserId);
    }


}