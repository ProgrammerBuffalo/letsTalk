using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.ServiceModel;
using System.Transactions;
using System.Linq;
using Microsoft.SqlServer.Server;
using System.Xml;
using System.Xml.Linq;

namespace letsTalk
{
    // Реализация логики сервера
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, // Single -> Объект ChatService является синглтоном
                    IncludeExceptionDetailInFaults = true, // Faults == Exceptions
                    ConcurrencyMode = ConcurrencyMode.Multiple)] // Multiple => Сервер должен держать нескольких пользователей себе (Под каждого юзера свой поток)
    public class ChatService : IChatService, IFileService, IAvatarService, IUnitService
    {
        //Строка для подключения к БД
        private static string connection_string = @"Server=(local);Database=MessengerDB;Integrated Security=true;";

        // Все подключенные пользователи, и чатрумы в них
        private Dictionary<ConnectedUser, List<int>> chatroomsInUsers = new Dictionary<ConnectedUser, List<int>>();

        //Нужен для синхронизации
        private object lockerSyncObj = new object();

        // Авторизация на сервер, метод ищет пользователя в БД
        public ServerUserInfo Authorization(AuthenticationUserInfo authenticationUserInfo)
        {
            ServerUserInfo serverUserInfo = null;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(@"SELECT* FROM Users WHERE [Login] = @Login
                                                                           AND [Password] = @Password", sqlConnection);

                    sqlCommand.Parameters.Add("@Login", SqlDbType.NVarChar).Value = authenticationUserInfo.Login;
                    sqlCommand.Parameters.Add("@Password", SqlDbType.NVarChar).Value = authenticationUserInfo.Password;

                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            AuthorizationExceptionFault authorizationExceptionFault = new AuthorizationExceptionFault();
                            throw new FaultException<AuthorizationExceptionFault>(authorizationExceptionFault, authorizationExceptionFault.Message);
                        }

                        while (reader.Read())
                        {

                            serverUserInfo = new ServerUserInfo()
                            {
                                SqlId = int.Parse(reader["Id"].ToString()),
                                Name = reader["Name"].ToString()
                            };
                        }
                    }
                }
                Console.WriteLine("User is authorized (" + serverUserInfo.SqlId + ")");

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }   
            catch (Exception ex)
            {
                throw ex;
            }
            return serverUserInfo;
        }

        // Регистрация пользователя, добавление нового пользователя в БД
        public int Registration(ServerUserInfo serverUserInfo)
        {
            SqlTransaction sqlTransaction = null;
            SqlConnection sqlConnection = null;

            int UserId = -1;

            try
            {
                sqlConnection = new SqlConnection(connection_string);

                sqlConnection.Open();

                sqlTransaction = sqlConnection.BeginTransaction();

                SqlCommand sqlCommandLogin = new SqlCommand(@"SELECT [Login] FROM Users WHERE [Login] = @Login", sqlConnection);
                sqlCommandLogin.Transaction = sqlTransaction;
                sqlCommandLogin.CommandType = CommandType.Text;
                sqlCommandLogin.Parameters.AddWithValue("@Login", serverUserInfo.Login);

                using (SqlDataReader reader = sqlCommandLogin.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        LoginExceptionFault loginExceptionFault = new LoginExceptionFault();
                        throw new FaultException<LoginExceptionFault>(loginExceptionFault, loginExceptionFault.Message);
                    }
                }

                SqlCommand sqlCommandName = new SqlCommand(@"SELECT [Name] FROM Users WHERE [Name] = @Name", sqlConnection);
                sqlCommandName.Transaction = sqlTransaction;
                sqlCommandName.CommandType = CommandType.Text;
                sqlCommandName.Parameters.AddWithValue("@Name", serverUserInfo.Name);

                using (SqlDataReader reader = sqlCommandName.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        NicknameExceptionFault nicknameExceptionFault = new NicknameExceptionFault();
                        throw new FaultException<NicknameExceptionFault>(nicknameExceptionFault, nicknameExceptionFault.Message);
                    }
                }

                SqlCommand sqlCommandInsertUser = new SqlCommand(@"INSERT INTO Users(Name, Login, Password) VALUES(@Name, @Login, @Password); SELECT SCOPE_IDENTITY()", sqlConnection);
                sqlCommandInsertUser.Transaction = sqlTransaction;
                sqlCommandInsertUser.CommandType = CommandType.Text;
                sqlCommandInsertUser.Parameters.AddWithValue("@Name", serverUserInfo.Name);
                sqlCommandInsertUser.Parameters.AddWithValue("@Login", serverUserInfo.Login);
                sqlCommandInsertUser.Parameters.AddWithValue("@Password", serverUserInfo.Password);

                UserId = int.Parse(sqlCommandInsertUser.ExecuteScalar().ToString());

                sqlTransaction.Commit();
            }
            catch (SqlException sqlEx)
            {
                sqlTransaction.Rollback();
                Console.WriteLine("Rollback sql");
                Console.WriteLine(sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sqlConnection.Close();
            }

            Console.WriteLine("User with nickname: " + serverUserInfo.Name + " is registered");
            return UserId;
        }

        private DownloadFileInfo AvatarDownload(string selector, int id)
        {
            DownloadFileInfo downloadFileInfo = new DownloadFileInfo();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandFindAvatar = new SqlCommand($@"SELECT stream_id FROM {selector} WHERE {selector}.Id = @Id", sqlConnection);
                    sqlCommandFindAvatar.CommandType = CommandType.Text;
                    sqlCommandFindAvatar.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                    var stream_id = sqlCommandFindAvatar.ExecuteScalar();

                    if (stream_id.GetType() == typeof(DBNull))
                    {
                        return downloadFileInfo;
                    }

                    SqlCommand sqlCommandTakeAvatar = new SqlCommand($@"SELECT* FROM GetFile(@stream_id)", sqlConnection);
                    sqlCommandTakeAvatar.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = (Guid)stream_id;

                    using (SqlDataReader reader = sqlCommandTakeAvatar.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var stream = new FileStream(reader[0].ToString(), FileMode.Open, FileAccess.Read);

                            downloadFileInfo.FileExtension = reader[1].ToString();
                            downloadFileInfo.Length = long.Parse(reader[2].ToString());
                            downloadFileInfo.FileStream = stream;
                        }
                    }
                }
            }
            catch (SqlException sqlEx) { Console.WriteLine("SqlException: " + sqlEx.Message); }
            catch (IOException ioEx) { Console.WriteLine("IOException: " + ioEx.Message); }
            catch (Exception ex) { Console.WriteLine("Exception: " + ex.Message); }

            return downloadFileInfo;
        }

        // Сервер отправляет аватарку зарегистированного пользователя в БД (Метод ищет аватарку пользователя, посредством связей в БД.
        // После того, как аватарка была найдена в БД, у нас открывается поток под эту картинку для того чтобы клиентская часть сегментами подгрузила её)
        public DownloadFileInfo UserAvatarDownload(DownloadRequest request)
        {
            return AvatarDownload("Users", request.Requested_SqlId);
        }

        public DownloadFileInfo ChatAvatarDownload(DownloadRequest request)
        {
            return AvatarDownload("Chatrooms", request.Requested_SqlId);
        }

        // Здесь сервер заносит картинку в файловую таблицу, процесс обратный методу AvatarDownload
        private void AvatarUpload(string selector, UploadFileInfo uploadRequest)
        {
            try
            {
                using (TransactionScope trScope = new TransactionScope())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                    {
                        sqlConnection.Open();

                        SqlCommand sqlCommandAddAvatar = new SqlCommand($@" INSERT INTO DataFT(file_stream, name, path_locator)
                                                                        OUTPUT INSERTED.stream_id, GET_FILESTREAM_TRANSACTION_CONTEXT(),
                                                                        INSERTED.file_stream.PathName()
                                                                        VALUES(CAST('' as varbinary(MAX)), @name, dbo.GetPathLocatorForChild('{selector}Avatars'))", sqlConnection);

                        sqlCommandAddAvatar.CommandType = CommandType.Text;

                        string fileName = uploadRequest.FileName;
                        sqlCommandAddAvatar.Parameters.Add("@name", SqlDbType.NVarChar).Value = "AVATAR" + uploadRequest.Responsed_SqlId + $".{fileName.Substring(fileName.LastIndexOf(".") + 1)}";

                        Guid stream_id;
                        byte[] transaction_context;
                        string full_path;

                        using (SqlDataReader sqlDataReader = sqlCommandAddAvatar.ExecuteReader())
                        {
                            sqlDataReader.Read();
                            stream_id = sqlDataReader.GetSqlGuid(0).Value;
                            transaction_context = sqlDataReader.GetSqlBinary(1).Value;
                            full_path = sqlDataReader.GetSqlString(2).Value;
                        }

                        const int bufferSize = 2048;

                        using (SqlFileStream sqlFileStream = new SqlFileStream(full_path, transaction_context, FileAccess.Write))
                        {
                            int bytesRead = 0;
                            var buffer = new byte[bufferSize];

                            while ((bytesRead = uploadRequest.FileStream.Read(buffer, 0, bufferSize)) > 0)
                            {
                                sqlFileStream.Write(buffer, 0, bytesRead);
                                sqlFileStream.Flush();
                            }

                        }

                        SqlCommand UpdateUserAvatar = new SqlCommand($@" UPDATE {selector} SET stream_id = @stream_id WHERE Id = @Id", sqlConnection);

                        UpdateUserAvatar.CommandType = CommandType.Text;
                        UpdateUserAvatar.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = stream_id;
                        UpdateUserAvatar.Parameters.Add("@Id", SqlDbType.Int).Value = uploadRequest.Responsed_SqlId;

                        UpdateUserAvatar.ExecuteNonQuery();

                        trScope.Complete();

                        Console.WriteLine($"avatar for {selector} {uploadRequest.Responsed_SqlId} is added");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StreamExceptionFault streamExceptionFault = new StreamExceptionFault();

                throw new FaultException<StreamExceptionFault>(streamExceptionFault, streamExceptionFault.Message);
            }
        }

        public void UserAvatarUpload(UploadFileInfo uploadRequest)
        {
            AvatarUpload("Users", uploadRequest);
        }

        public void ChatAvatarUpload(UploadFileInfo uploadRequest)
        {
            AvatarUpload("Chatrooms", uploadRequest);
        }

        // Получает список всех существующий пользователей в Базе Данных
        public Dictionary<int, string> GetRegisteredUsers(int count, int offset, int callerId)
        {
            Dictionary<int, string> users = new Dictionary<int, string>();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandShowMoreUsers = new SqlCommand(@"SELECT Id, Name FROM ShowMoreUsers(@count, @offset, @callerId)", sqlConnection);
                    sqlCommandShowMoreUsers.CommandType = CommandType.Text;

                    sqlCommandShowMoreUsers.Parameters.Add("@count", SqlDbType.SmallInt).Value = count;
                    sqlCommandShowMoreUsers.Parameters.Add("@offset", SqlDbType.SmallInt).Value = offset;
                    sqlCommandShowMoreUsers.Parameters.Add("@callerId", SqlDbType.SmallInt).Value = callerId;

                    using (SqlDataReader sqlDataReader = sqlCommandShowMoreUsers.ExecuteReader())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            while (sqlDataReader.Read())
                            {
                                users.Add(sqlDataReader.GetSqlInt32(0).Value, sqlDataReader.GetSqlString(1).Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return users;
        }

        // Создает чатрум с выбранными пользователями
        public int CreateChatroom(string chatName, List<int> users)
        {
            int chat_id;
            string fullpathXML = "";
            try
            {
                Console.WriteLine("Creating chatroom (" + OperationContext.Current.Channel.GetHashCode() + ")");

                using (TransactionScope trScope = new TransactionScope())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                    {
                        sqlConnection.Open();

                        SqlCommand sqlCommandAddChatroom = new SqlCommand(@"INSERT INTO Chatrooms([Name]) OUTPUT INSERTED.Id VALUES(@chatName)", sqlConnection);

                        sqlCommandAddChatroom.CommandType = CommandType.Text;
                        sqlCommandAddChatroom.Parameters.Add("@chatName", SqlDbType.NVarChar).Value = chatName;

                        chat_id = int.Parse(sqlCommandAddChatroom.ExecuteScalar().ToString());

                        SqlCommand sqlCommandAddUsersToChatroom = new SqlCommand("AddUsersToChat", sqlConnection);
                        sqlCommandAddUsersToChatroom.CommandType = CommandType.StoredProcedure;

                        SqlMetaData sqlMetaData = new SqlMetaData("UserId", SqlDbType.Int);
                        List<SqlDataRecord> usersRecords = new List<SqlDataRecord>(users.Count);

                        foreach (var user in users)
                        {
                            SqlDataRecord sqlDataRecord = new SqlDataRecord(sqlMetaData);
                            sqlDataRecord.SetInt32(0, user);

                            usersRecords.Add(sqlDataRecord);
                        }

                        var parameter = new SqlParameter("@Users", SqlDbType.Structured);
                        parameter.TypeName = "UsersTableType";
                        parameter.Value = usersRecords;

                        sqlCommandAddUsersToChatroom.Parameters.Add(parameter);
                        sqlCommandAddUsersToChatroom.Parameters.Add("@ChatID", SqlDbType.Int).Value = chat_id;

                        sqlCommandAddUsersToChatroom.ExecuteNonQuery();

                        SqlCommand sqlCommandAddFileForContentXML = new SqlCommand(@"INSERT INTO DataFT(file_stream, name, path_locator)                                                                                  
                                                                                     OUTPUT INSERTED.stream_id
                                                                                     VALUES(CAST('' as varbinary(MAX)), @name, dbo.GetPathLocatorForChild('Messages'))", sqlConnection);

                        sqlCommandAddFileForContentXML.CommandType = CommandType.Text;

                        sqlCommandAddFileForContentXML.Parameters.Add("@name", SqlDbType.NVarChar).Value = "CHAT" + chat_id.ToString() + ".xml";

                        Guid stream_id = new Guid();

                        using (SqlDataReader sqlDataReader = sqlCommandAddFileForContentXML.ExecuteReader())
                        {
                            while (sqlDataReader.Read())
                            {
                                stream_id = sqlDataReader.GetSqlGuid(0).Value;
                            }
                        }

                        SqlCommand sqlCommandMergeContentWithXML = new SqlCommand(@"INSERT INTO Contents 
                                                                                    OUTPUT INSERTED.Id
                                                                                    VALUES(@stream_id)", sqlConnection);

                        sqlCommandMergeContentWithXML.CommandType = CommandType.Text;

                        sqlCommandMergeContentWithXML.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = stream_id;

                        int content_id = 0;
                        using (SqlDataReader sqlDataReader = sqlCommandMergeContentWithXML.ExecuteReader())
                        {
                            sqlDataReader.Read();
                            content_id = sqlDataReader.GetSqlInt32(0).Value;
                        }

                        SqlCommand sqlCommandMergeContentWithChatroom = new SqlCommand(@"INSERT INTO Chatroom_Content VALUES(@IdChat, @IdContent)", sqlConnection);
                        sqlCommandMergeContentWithChatroom.CommandType = CommandType.Text;

                        sqlCommandMergeContentWithChatroom.Parameters.Add("@IdChat", SqlDbType.Int).Value = chat_id;
                        sqlCommandMergeContentWithChatroom.Parameters.Add("@IdContent", SqlDbType.Int).Value = content_id;

                        sqlCommandMergeContentWithChatroom.ExecuteNonQuery();

                        SqlCommand sqlCommandTakeXML = new SqlCommand(@"SELECT dbo.GetXMLFile(@chatId)", sqlConnection);

                        sqlCommandTakeXML.Parameters.Add("@chatId", SqlDbType.Int).Value = chat_id;
                        fullpathXML = sqlCommandTakeXML.ExecuteScalar().ToString();

                    }
                    trScope.Complete();
                }
                lock (lockerSyncObj)
                {
                    XmlCreation(fullpathXML);

                    Console.WriteLine("Users added to chat callbacks...");

                    foreach (ConnectedUser connectedUser in chatroomsInUsers.Keys)
                    {
                        foreach (var user in users)
                        {
                            if (connectedUser.SqlID == user)
                            {

                                chatroomsInUsers[connectedUser].Add(chat_id);
                                if (connectedUser.UserContext != OperationContext.Current)
                                    connectedUser.UserContext.GetCallbackChannel<IChatCallback>().NotifyUserIsAddedToChat(chat_id, users);
                            }

                        }
                    }
                }



                Console.WriteLine("Chatroom has been created (" + OperationContext.Current.Channel.GetHashCode() + ")");
                return chat_id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        //Создание Xml-файла для чатрума
        private void XmlCreation(string path)
        {
            if (new FileInfo(path).Length <= 0)
            {
                Console.WriteLine("Creating declaration for XML");
                XmlDocument document = new XmlDocument();
                XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
                document.AppendChild(declaration);
                XmlElement root = document.CreateElement("Messages");
                document.AppendChild(root);
                document.Save(path);
                Console.WriteLine("Declaration has been created for XML");
            }
        }

        //Отправка текстового сообщения с чатрума
        public void SendMessageText(ServiceMessageText message, int chatroomId)
        {
            Console.WriteLine("Sending message to server (" + OperationContext.Current.Channel.GetHashCode() + ")");


            List<ConnectedUser> users = chatroomsInUsers.Where(chatrooms => chatrooms.Value
                                                        .Contains(chatroomId))
                                                        .Select(u => u.Key).ToList();

            string fullpath;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommandFindXMLFromContentsTable = new SqlCommand(@"SELECT dbo.GetXMLFile(@chatId)", sqlConnection);

                    sqlCommandFindXMLFromContentsTable.Parameters.Add("@chatId", SqlDbType.Int).Value = chatroomId;
                    fullpath = sqlCommandFindXMLFromContentsTable.ExecuteScalar().ToString();

                }

                lock (lockerSyncObj)
                {
                    AddToXML(fullpath, message);
                    Console.WriteLine("Sending message callbacks...");
                    foreach (ConnectedUser user in users)
                    {
                        if (user.UserContext.Channel != OperationContext.Current.Channel)
                            user.UserContext.GetCallbackChannel<IChatCallback>().ReplyMessage(message, chatroomId);
                    }
                }

                Console.WriteLine("Message has been sent (" + OperationContext.Current.Channel.GetHashCode() + ")");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Добавление узла в XML-файл
        private void AddToXML(string full_path, ServiceMessage serviceMessage)
        {
            Console.WriteLine("Adding message to XML (" + OperationContext.Current.Channel.GetHashCode() + ")");
            if (!File.Exists(full_path))
            {
                throw new Exception("File xml doesnt exsists!");
            }
            XDocument xDoc = XDocument.Load(full_path);

            XElement xMessageEl = new XElement("Message");
            XAttribute xNameAttr;
            XElement xDateEl;
            XElement xSenderEl;

            if (serviceMessage is ServiceMessageText)
            {
                ServiceMessageText serviceMessageText = serviceMessage as ServiceMessageText;
                xNameAttr = new XAttribute("type", "text");
                xSenderEl = new XElement("Sender", serviceMessageText.Sender);
                xDateEl = new XElement("Date", DateTime.Now.ToString());
                XElement xTextEl = new XElement("Text", serviceMessageText.Text);
                xMessageEl.Add(xTextEl);
            }
            else
            {
                ServiceMessageFile serviceMessageFile = serviceMessage as ServiceMessageFile;
                xNameAttr = new XAttribute("type", "file");
                xSenderEl = new XElement("Sender", serviceMessageFile.Sender);
                xDateEl = new XElement("Date", DateTime.Now.ToString());
                XElement xStreamIdEl = new XElement("StreamId", serviceMessageFile.StreamId);
                XElement xFileName = new XElement("FileName", serviceMessageFile.FileName);
                xMessageEl.Add(xStreamIdEl);
                xMessageEl.Add(xFileName);
            }
            xMessageEl.Add(xNameAttr);
            xMessageEl.Add(xSenderEl);
            xMessageEl.Add(xDateEl);

            xDoc.Root.Add(xMessageEl);

            xDoc.Save(full_path);
            Console.WriteLine("Message is added to XML (" + OperationContext.Current.Channel.GetHashCode() + ")");
        }

        //Нахождение сообщений чатрума в XML-файле
        private List<ServiceMessage> FindXmlNodes(string fullpath)
        {
            XDocument xDocument = XDocument.Load(fullpath);
            List<ServiceMessage> serviceMessages = new List<ServiceMessage>();

            foreach (var xMessage in xDocument.Element("Messages").Elements("Message"))
            {
                if (xMessage.Attribute("type").Value.Equals("text"))
                {
                    serviceMessages.Add(new ServiceMessageText
                    {
                        Sender = int.Parse(xMessage.Element("Sender").Value),
                        DateTime = DateTime.Parse(xMessage.Element("Date").Value),
                        Text = xMessage.Element("Text").Value
                    });
                }
                else
                {
                    serviceMessages.Add(new ServiceMessageFile
                    {
                        Sender = int.Parse(xMessage.Element("Sender").Value),
                        DateTime = DateTime.Parse(xMessage.Element("Date").Value),
                        StreamId = Guid.Parse(xMessage.Element("StreamId").Value),
                        FileName = xMessage.Element("FileName").Value
                    });
                }
            }
            return serviceMessages;
        }

        //Оповещение серверу о том, что пишется на данный момент сообщение
        public void MessageIsWriting(int chatroomId, Nullable<int> userSqlId)
        {
            List<ConnectedUser> users = chatroomsInUsers.Where(chatrooms => chatrooms.Value
                                                        .Contains(chatroomId))
                                                        .Select(u => u.Key).ToList();

            Console.WriteLine("Message is writing callbacks...");

            foreach (ConnectedUser user in users)
            {
                if (user.UserContext.Channel != OperationContext.Current.Channel)
                {
                    user.UserContext.GetCallbackChannel<IChatCallback>().ReplyMessageIsWriting(userSqlId, chatroomId);
                }
            }
        }

        //Подключение клиента к серверу, получение списка чатрумов и пользователей в нем
        public void Connect(int sqlId, string userName)
        {
            if (!chatroomsInUsers.Keys.Any(u => u.SqlID == sqlId))
            {
                lock (lockerSyncObj)
                {
                    chatroomsInUsers.Add(new ConnectedUser()
                    {
                        SqlID = sqlId,
                        Name = userName,
                        UserContext = OperationContext.Current
                    }, FindAllChatroomsForServer(sqlId));

                    OperationContext.Current.Channel.Faulted += Channel_Closed;

                    try
                    {
                        Console.WriteLine("User in connected callbacks...");

                        lock (lockerSyncObj)
                        {
                            foreach (ConnectedUser user in chatroomsInUsers.Keys)
                            {
                                if (user.SqlID == sqlId)
                                    continue;

                                OperationContext userContext = user.UserContext;
                                userContext.GetCallbackChannel<IChatCallback>().NotifyUserIsOnline(sqlId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }

                Console.WriteLine("User is connected(" + OperationContext.Current.Channel.GetHashCode() + ")");
            }
            else
            {
                ConnectionExceptionFault connectionExceptionFault = new ConnectionExceptionFault();
                throw new FaultException<ConnectionExceptionFault>(connectionExceptionFault, connectionExceptionFault.Message);
            }
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Disconnect(sender as IContextChannel);
        }

        //Отключение пользователя
        private void Disconnect(IContextChannel clientChannel)
        {
            ConnectedUser discUser = chatroomsInUsers.Keys.FirstOrDefault(u => u.UserContext.Channel == clientChannel);
            if (discUser != null)
            {
                lock (lockerSyncObj)
                {
                    chatroomsInUsers.Remove(discUser);
                    Console.WriteLine("User is offline callbacks...");
                    lock (lockerSyncObj)
                    {
                        foreach (var user in chatroomsInUsers.Keys)
                        {
                            OperationContext userContext = user.UserContext;
                            userContext.GetCallbackChannel<IChatCallback>().NotifyUserIsOffline(discUser.SqlID);
                        }
                    }
                }
            }
            Console.WriteLine("User is disconnected (" + clientChannel.GetHashCode() + ")");
        }

        //Поиск всех чатрумов для клиента во время подключения к серверу
        public Dictionary<Chatroom, List<UserInChat>> FindAllChatroomsForClient(int userSqlId)
        {
            Dictionary<Chatroom, List<UserInChat>> usersInChatroom = new Dictionary<Chatroom, List<UserInChat>>();
            try
            {
                Console.WriteLine("Finding chatrooms for user" + "(" + OperationContext.Current.Channel.GetHashCode() + ")");

                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandFindChatrooms = new SqlCommand(@"SELECT* FROM GetChatroomsForUser(@userId)", sqlConnection);

                    sqlCommandFindChatrooms.CommandType = CommandType.Text;
                    sqlCommandFindChatrooms.Parameters.Add("@userId", SqlDbType.Int).Value = userSqlId;

                    using (SqlDataReader sqlDataReader = sqlCommandFindChatrooms.ExecuteReader())
                    {
                        while (sqlDataReader.Read())
                        {
                            int chatId = sqlDataReader.GetInt32(0);
                            string chatName = sqlDataReader.GetSqlString(1).Value;
                            int userId = sqlDataReader.GetInt32(2);
                            string userName = sqlDataReader.GetSqlString(3).Value;

                            Chatroom chat = usersInChatroom.Keys.FirstOrDefault(c => c.ChatSqlId == chatId);

                            bool isOnline = chatroomsInUsers.Any(cU => cU.Key.SqlID == userId);

                            if (chat == null)
                            {
                                usersInChatroom.Add(new Chatroom()
                                {
                                    ChatSqlId = chatId,
                                    ChatName = chatName
                                },
                                new List<UserInChat>() { new UserInChat() {
                                                          UserSqlId = userId,
                                                          UserName = userName,
                                                          IsOnline = isOnline } });
                            }
                            else
                            {
                                usersInChatroom[chat].Add(new UserInChat
                                {
                                    UserSqlId = userId,
                                    UserName = userName,
                                    IsOnline = isOnline
                                });
                            }
                        }
                    }
                }
                Console.WriteLine("Chatrooms are found" + "(" + OperationContext.Current.Channel.GetHashCode() + ")");
                return usersInChatroom;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return usersInChatroom;
        }

        //Поиск всех чатрумов в которых находится подключенный клиент
        private List<int> FindAllChatroomsForServer(int sqlId)
        {
            Console.WriteLine("Adding chatrooms to server (" + OperationContext.Current.Channel.GetHashCode() + ")");
            List<int> chatrooms = new List<int>();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand FindChatroomsCommand = new SqlCommand(@"SELECT Id_Chat FROM User_Chatroom WHERE Id_User = @Id_User", sqlConnection);
                    FindChatroomsCommand.CommandType = CommandType.Text;
                    FindChatroomsCommand.Parameters.Add("@Id_User", SqlDbType.Int).Value = sqlId;

                    using (SqlDataReader reader = FindChatroomsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chatrooms.Add(reader.GetInt32(0));
                        }
                    }
                }
                Console.WriteLine("Chatrooms were added to server (" + OperationContext.Current.Channel.GetHashCode() + ")");
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine(sqlEx.Message);
            }

            return chatrooms;
        }

        //Добавление пользователя в чатрум
        public void AddUserToChatroom(int userId, int chatId)
        {
            try
            {
                Console.WriteLine("Adding user to chatroom (" + OperationContext.Current.Channel.GetHashCode() + ")");
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandAddUserToChat = new SqlCommand(@"INSERT INTO User_Chatroom VALUES(@ChatID, @UserID)", sqlConnection);
                    sqlCommandAddUserToChat.CommandType = CommandType.Text;

                    sqlCommandAddUserToChat.Parameters.Add("@ChatID", SqlDbType.Int).Value = chatId;
                    sqlCommandAddUserToChat.Parameters.Add("@UserID", SqlDbType.Int).Value = userId;

                    sqlCommandAddUserToChat.ExecuteNonQuery();
                }

                if (chatroomsInUsers.Keys.Any(u => u.SqlID == userId))
                {
                    List<ConnectedUser> users = chatroomsInUsers.Where(chatrooms => chatrooms.Value
                                                                .Contains(chatId))
                                                                .Select(u => u.Key).ToList();

                    ConnectedUser connectedUser = chatroomsInUsers.Keys.First(u => u.SqlID == userId);
                    connectedUser.UserContext.GetCallbackChannel<IChatCallback>().NotifyUserIsAddedToChat(chatId, users.Select(u => u.SqlID).ToList());

                    Console.WriteLine("User joined callbacks...");

                    foreach (var user in users)
                    {
                        if (user.UserContext.Channel != OperationContext.Current.Channel)
                            user.UserContext.GetCallbackChannel<IChatCallback>().UserJoinedToChatroom(userId);
                    }
                }
                Console.WriteLine("User has been added to chatroom (" + OperationContext.Current.Channel.GetHashCode() + ")");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        //Удаление пользователя из чатрума
        public void RemoveUserFromChatroom(int userId, int chatId)
        {
            try
            {
                Console.WriteLine("Removing user from chatroom (" + OperationContext.Current.Channel.GetHashCode() + ")");

                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(@"DELETE FROM User_Chatroom WHERE User Id_Chat = @IdChat AND Id_User = @IdUser", sqlConnection);

                    sqlCommand.Parameters.Add("@IdChat", SqlDbType.Int).Value = chatId;
                    sqlCommand.Parameters.Add("@IdUser", SqlDbType.Int).Value = userId;

                    sqlCommand.ExecuteNonQuery();
                }

                ConnectedUser connectedUser = chatroomsInUsers.Keys.FirstOrDefault(u => u.SqlID == userId);

                lock (lockerSyncObj)
                {
                    if (connectedUser != null)
                    {
                        connectedUser.UserContext.GetCallbackChannel<IChatCallback>().NotifyUserIsRemovedFromChat(chatId);
                        chatroomsInUsers.Where(users => users.Key.SqlID == userId)
                                        .Select(c => c.Value)
                                        .First().Remove(chatId);
                    }

                    Console.WriteLine("User left callbacks...");

                    foreach (var user in chatroomsInUsers.Keys)
                    {
                        if (user.UserContext.Channel != OperationContext.Current.Channel)
                            user.UserContext.GetCallbackChannel<IChatCallback>().UserLeftChatroom(userId);
                    }
                }
                Console.WriteLine("User was removed from chatroom (" + OperationContext.Current.Channel.GetHashCode() + ")");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Полное удаление чатрума
        public void DeleteChatroom(int chatId)
        {
            try
            {
                Console.WriteLine("Deleting chatroom (" + OperationContext.Current.Channel.GetHashCode() + ")");

                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandDeleteChatroom = new SqlCommand(@"DELETE FROM User_Chatroom WHERE ChatID = @ChatID", sqlConnection);
                    sqlCommandDeleteChatroom.CommandType = CommandType.Text;

                    sqlCommandDeleteChatroom.Parameters.Add("@ChatID", SqlDbType.Int).Value = chatId;
                    sqlCommandDeleteChatroom.ExecuteNonQuery();

                    lock (lockerSyncObj)
                    {
                        foreach (ConnectedUser user in chatroomsInUsers.Keys)
                        {
                            if (user.UserContext.Channel != OperationContext.Current.Channel)
                            {
                                user.UserContext.GetCallbackChannel<IChatCallback>().NotifyUserIsRemovedFromChat(chatId);
                            }
                            chatroomsInUsers[user].Remove(chatId);
                        }

                    }
                }
                Console.WriteLine("Chatroom is deleted (" + OperationContext.Current.Channel.GetHashCode() + ")");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //Отправка файла с чатрума на сервер
        public FileFromChatDownloadRequest FileUpload(UploadFromChatToServer chatToServer)
        {
            Guid stream_id = new Guid();
            string fullpathXML = "";
            string fileName = "";

            FileFromChatDownloadRequest fileFromChatDownloadRequest = new FileFromChatDownloadRequest();

            try
            {
                using (TransactionScope transactionScope = new TransactionScope())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                    {
                        sqlConnection.Open();

                        SqlCommand sqlCommandAddFileToDataFT = new SqlCommand($@" INSERT INTO DataFT(file_stream, name, path_locator)
                                                                                  OUTPUT INSERTED.stream_id, GET_FILESTREAM_TRANSACTION_CONTEXT(),
                                                                                  INSERTED.file_stream.PathName()
                                                                                  VALUES(CAST('' as varbinary(MAX)), @name, dbo.GetPathLocatorForChild('MessageFiles'))", sqlConnection);

                        sqlCommandAddFileToDataFT.CommandType = CommandType.Text;

                        fileName = chatToServer.FileName.Substring(chatToServer.FileName.LastIndexOf(@"\") + 1);
                        string full_path = "";

                        sqlCommandAddFileToDataFT.Parameters.Add("@name", SqlDbType.NVarChar).Value = fileName.Substring(fileName.LastIndexOf("\\") + 1);

                        byte[] transaction_context = null;
                        try
                        {
                            using (SqlDataReader sqlDataReader = sqlCommandAddFileToDataFT.ExecuteReader())
                            {
                                sqlDataReader.Read();
                                stream_id = sqlDataReader.GetSqlGuid(0).Value;
                                transaction_context = sqlDataReader.GetSqlBinary(1).Value;
                                full_path = sqlDataReader.GetSqlString(2).Value;
                            }
                        }
                        catch (SqlException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        const int bufferSize = 2048;

                        using (SqlFileStream sqlFileStream = new SqlFileStream(full_path, transaction_context, FileAccess.Write))
                        {
                            int bytesRead = 0;
                            var buffer = new byte[bufferSize];

                            while ((bytesRead = chatToServer.FileStream.Read(buffer, 0, bufferSize)) > 0)
                            {
                                sqlFileStream.Write(buffer, 0, bytesRead);
                                sqlFileStream.Flush();
                            }

                        }

                        SqlCommand updateFileName = new SqlCommand($@"UPDATE DataFT SET name = '{stream_id}' + '_' + name WHERE stream_id = @stream_id", sqlConnection);
                        updateFileName.CommandType = CommandType.Text;
                        updateFileName.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = stream_id;
                        updateFileName.ExecuteNonQuery();

                        SqlCommand sqlCommandTakeXML = new SqlCommand(@"SELECT dbo.GetXMLFile(@chatId)", sqlConnection);

                        sqlCommandTakeXML.Parameters.Add("@chatId", SqlDbType.Int).Value = chatToServer.ChatroomId;
                        fullpathXML = sqlCommandTakeXML.ExecuteScalar().ToString();
                        
                    }
                    transactionScope.Complete();
                }

                lock (lockerSyncObj)
                {
                    ServiceMessageFile serviceMessageFile = new ServiceMessageFile
                    {
                        Sender = chatToServer.Responsed_UserSqlId,
                        StreamId = stream_id,
                        DateTime = DateTime.Now,
                        FileName = fileName
                    };

                    AddToXML(fullpathXML, serviceMessageFile);

                    foreach (ConnectedUser user in chatroomsInUsers.Keys)
                    {
                        if (user.SqlID != chatToServer.Responsed_UserSqlId)
                        {
                            user.UserContext.GetCallbackChannel<IChatCallback>().NotifyUserFileSendedToChat(serviceMessageFile, chatToServer.ChatroomId);
                        }

                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StreamExceptionFault streamExceptionFault = new StreamExceptionFault();

                throw new FaultException<StreamExceptionFault>(streamExceptionFault, streamExceptionFault.Message);
            }
            fileFromChatDownloadRequest.StreamId = stream_id;

            return fileFromChatDownloadRequest;
        }

        //Загрузка файла с чатрума
        public DownloadFileInfo FileDownload(FileFromChatDownloadRequest request)
        {
            DownloadFileInfo downloadFileInfo = new DownloadFileInfo();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandFindFile = new SqlCommand($@"SELECT* FROM GetFile(@stream_id)", sqlConnection);
                    sqlCommandFindFile.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = request.StreamId;

                    using (SqlDataReader reader = sqlCommandFindFile.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var stream = new FileStream(reader[0].ToString(), FileMode.Open, FileAccess.Read);

                            downloadFileInfo.FileExtension = reader[1].ToString();
                            downloadFileInfo.Length = long.Parse(reader[2].ToString());
                            downloadFileInfo.FileStream = stream;
                        }
                    }
                }
            }
            catch (SqlException sqlEx) { Console.WriteLine("SqlException: " + sqlEx.Message); }
            catch (IOException ioEx) { Console.WriteLine("IOException: " + ioEx.Message); }
            catch (Exception ex) { Console.WriteLine("Exception: " + ex.Message); }

            return downloadFileInfo;
        }

        //Загрузка сообщений с одного чатрума
        public List<ServiceMessage> MessagesFromOneChat(int chatroomId)
        {
            List<ServiceMessage> messages = null;
            string fullpathXML;
            try
            {
                Console.WriteLine("Finding messages from chatroom " + chatroomId);

                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCommandTakeXML = new SqlCommand(@"SELECT dbo.GetXMLFile(@chatId)", sqlConnection);

                    sqlCommandTakeXML.Parameters.Add("@chatId", SqlDbType.Int).Value = chatroomId;
                    fullpathXML = sqlCommandTakeXML.ExecuteScalar().ToString();
                }

                messages = FindXmlNodes(fullpathXML);

                Console.WriteLine("Messages were found from chatroom " + chatroomId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return messages;
        }
    }
}