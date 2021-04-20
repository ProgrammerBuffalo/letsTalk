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

    [ServiceContract(Name = "Chat", Namespace = "letsTalk.IChatService",
                     CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(LoginExceptionFault))]
        [FaultContract(typeof(NicknameExceptionFault))]
        int Registration(ServerUserInfo serverUserInfo);

        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(AuthorizationExceptionFault))]
        ServerUserInfo Authorization(AuthenticationUserInfo authenticationUserInfo);

        [OperationContract(IsOneWay = true)]
        void MessageIsWriting(int chatroomId, int userSqlId);

        [OperationContract(IsOneWay = false)]
        void SendMessageText(ServiceMessageText message, int chatroomId);

        [OperationContract(IsOneWay = false)]
        Dictionary<int, string> GetRegisteredUsers(int count, int offset, int callerId);

        [OperationContract(IsOneWay = true)]
        void CreateChatroom(string chatName, List<int> users);

        [OperationContract(IsOneWay = true)]
        void DeleteChatroom(int chatId);

        [OperationContract(IsOneWay = false)]
        void AddUserToChatroom(int userId, int chatId);

        [OperationContract(IsOneWay = false)]
        void RemoveUserFromChatroom(int userId, int chatId);

        [OperationContract(IsOneWay = false)]
        [FaultContract(typeof(ConnectionExceptionFault))]
        Dictionary<int, List<int>> Connect(int sqlId, string userName);

        [OperationContract(IsOneWay = false)]
        List<ServiceMessage> MessagesFromOneChat(int chatroomId);

        [OperationContract]
        void Disconnect();
    }

    public interface IChatCallback
    {
        //Оповещение пользователей, что пользователь онлайн
        [OperationContract(IsOneWay = true)]
        void NotifyUserIsOnline(int sqlUserId);

        //Оповещение пользователей, что пользователь оффлайн
        [OperationContract(IsOneWay = true)]
        void NotifyUserIsOffline(int sqlUserId);

        //Оповещение пользователя, о том, что он был добавлен в чатрум
        [OperationContract(IsOneWay = true)]
        void NotifyUserIsAddedToChat(int chatId, List<int> usersInChat);

        //Оповещение пользователя о его удалении с чатрума
        [OperationContract(IsOneWay = true)]
        void NotifyUserIsRemovedFromChat(int chatId);

        //Оповещение пользователей, что пользователей присоединился/добавлен в чатрум
        [OperationContract(IsOneWay = true)]
        void UserJoinedToChatroom(int userId);

        //Оповещение пользователей, что пользователь покинул группу
        [OperationContract(IsOneWay = true)]
        void UserLeftChatroom(int userId);

        //Оповещение о добавлении текстового сообщения в чатрум
        [OperationContract(IsOneWay = true)]
        void ReplyMessage(ServiceMessageText message, int chatroomId);

        //Оповещение о том, что пользователь пишет для чатрума сообщение
        [OperationContract(IsOneWay = true)]
        void ReplyMessageIsWriting(int sqlId);

        //Оповещение пользователей, о добавлении в чатрум файла
        [OperationContract(IsOneWay = true)]
        void NotifyUserFileSendedToChat(ServiceMessageFile serviceMessageFile, int chatroomId);

    }

}