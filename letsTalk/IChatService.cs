using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace letsTalk
{
    // ИНТЕРФЕЙС РАБОТАЕТ НА БАЗЕ ПРОТОКОЛА TCP

    // IsOneWay подразумевает, что не будет callback'a от сервера
    // IsOneWay = false => Должен возвращаться ответ от сервера

    // ServiceContract подразумевает, что он будет работать на базе технологии WCF

    /* OperationContract подразумевает, что метод будет работать также на базе технологии WCF
    в случае, если у нас не будет данного атрибута, то метод будет считаться "обычным" (!Клиентский код не будет видеть "обычный" метод, 
    т.к. метаданные "обычного" метода не будут отправляется в клиентский код) */

    /* FaultContract -> всё равно что Exception, но при Fault у нас ошибка будет отправлятся клиенту (Если бы у нас не было,
       то пользователь просто бы не понимал какие ошибки могли бы возникнуть, т.к. Exception у нас бы просто обрабатывался локально в самом сервере)
       Примеры Fault исключений: *Пользователь не правильно ввёл пароль или логин при авторизации
                                 *Логин в бд уже такой существует при регистрации
                                 *Сервер не работает в данный момент 
                                 
       typeof'ом мы указываем сам объект исключения, который должен возвращаться клиенту*/

    // Для общения с методами данного интерфейса используется адрес net.tcp://localhost:8302/

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

        [OperationContract(IsOneWay = false)]
        bool SendMessage(string message);

        [OperationContract(IsOneWay = false)]
        Dictionary<int, string> GetUsers(int count, int offset, int callerId);

        [OperationContract(IsOneWay = true)]
        void CreateChatroom(string chatName, List<int> users);

        [OperationContract]
        void Disconnect(Guid UserId);
    }

}