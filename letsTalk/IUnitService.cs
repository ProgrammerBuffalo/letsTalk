﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace letsTalk
{
    [ServiceContract(Name = "Unit", Namespace = "letsTalk.IUnitService")]
    interface IUnitService
    {
        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(AuthorizationExceptionFault))]
        ServerUserInfo Authorization(AuthenticationUserInfo authenticationUserInfo);

        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(LoginExceptionFault))]
        [FaultContract(typeof(NicknameExceptionFault))]
        int Registration(ServerUserInfo serverUserInfo);

        [OperationContract(IsOneWay = false)]
        Dictionary<int, string> GetRegisteredUsers(int count, int offset, int callerId);

        [OperationContract(IsOneWay = false)]
        List<ServiceMessage> MessagesFromOneChat(int chatroomId, int userId, int offset, int count, DateTime offsetDate);
    }
}
