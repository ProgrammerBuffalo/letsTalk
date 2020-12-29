using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using System.Transactions;

namespace letsTalk
{
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                    IncludeExceptionDetailInFaults = true, 
                    ConcurrencyMode = ConcurrencyMode.Multiple)]
   public class ChatService : IChatService, IFileService
   {

        private static string connection_string = @"Server=(local);Database=MessengerDB;Integrated Security=true;";

        private Dictionary<Guid, ConnectedServerUser> connectedUsers = new Dictionary<Guid, ConnectedServerUser>();

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
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return serverUserInfo;
        }


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
            finally
            {
                sqlConnection.Close();
            }

            Console.WriteLine("User with nickname: " + serverUserInfo.Name + " is registered");
            return UserId;
        }

        public DownloadFileInfo AvatarDownload(DownloadRequest request)
        {
            DownloadFileInfo downloadFileInfo = null;
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                {
                    sqlConnection.Open();

                    SqlCommand sqlCommandFindAvatar = new SqlCommand(@"SELECT stream_id FROM Users WHERE Users.Id = @Id", sqlConnection);
                    sqlCommandFindAvatar.CommandType = CommandType.Text;
                    sqlCommandFindAvatar.Parameters.Add("@Id", SqlDbType.Int).Value = request.Requested_UserSqlId;

                    var stream_id = sqlCommandFindAvatar.ExecuteScalar();

                    if (stream_id.GetType() == typeof(DBNull))
                    {
                        throw new FileNotFoundException("Avatar not found");
                    }

                    SqlCommand sqlCommandTakeAvatar = new SqlCommand($@"SELECT* FROM GetAvatar(@stream_id)", sqlConnection);
                    sqlCommandTakeAvatar.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = (Guid)stream_id;

                    using (SqlDataReader reader = sqlCommandTakeAvatar.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            downloadFileInfo = new DownloadFileInfo();

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

        public void AvatarUpload(UploadFileInfo uploadResponse)
        {

            try
            {
                using (TransactionScope trScope = new TransactionScope())
                {
                    using (SqlConnection sqlConnection = new SqlConnection(connection_string))
                    {
                        sqlConnection.Open();

                        SqlCommand sqlCommandAddAvatar = new SqlCommand($@" INSERT INTO DataFT(file_stream, name)
                                                                        OUTPUT INSERTED.stream_id, GET_FILESTREAM_TRANSACTION_CONTEXT(),
                                                                        INSERTED.file_stream.PathName()
                                                                        VALUES(CAST('' as varbinary(MAX)), @name)", sqlConnection);

                        sqlCommandAddAvatar.CommandType = CommandType.Text;

                        sqlCommandAddAvatar.Parameters.Add("@name", SqlDbType.NVarChar).Value = "AVATAR" + uploadResponse.Responsed_UserSqlId + $".{uploadResponse.FileExtension}";

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

                            while ((bytesRead = uploadResponse.FileStream.Read(buffer, 0, bufferSize)) > 0)
                            {
                                sqlFileStream.Write(buffer, 0, bytesRead);
                                sqlFileStream.Flush();
                            }

                        }
                        
                        SqlCommand UpdateUserAvatar = new SqlCommand($@" UPDATE Users SET Users.stream_id = @stream_id WHERE Users.Id = @user_id", sqlConnection);

                        UpdateUserAvatar.CommandType = CommandType.Text;
                        UpdateUserAvatar.Parameters.Add("@stream_id", SqlDbType.UniqueIdentifier).Value = stream_id;
                        UpdateUserAvatar.Parameters.Add("@user_id", SqlDbType.Int).Value = uploadResponse.Responsed_UserSqlId;

                        UpdateUserAvatar.ExecuteNonQuery();

                        trScope.Complete();

                        Console.WriteLine($"avatar for user {uploadResponse.Responsed_UserSqlId} is added");
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally { if (uploadResponse.FileStream != null) uploadResponse.FileStream.Dispose(); }
        }

        public Guid Connect(int sqlId)
        {
            Guid uniqueId = Guid.NewGuid();

            ConnectedServerUser serverUser = new ConnectedServerUser()
            {
                SqlId = sqlId,
                OperationContext = OperationContext.Current
            };

            connectedUsers.Add(uniqueId, serverUser);
            Console.WriteLine($"User: {uniqueId} is Connected");

            return uniqueId;
        }

        public void Disconnect(Guid uniqueId)
        {
            connectedUsers.Remove(uniqueId);
            Console.WriteLine($"User: {uniqueId} is Disconnected");
        }
    }
}
