using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace Client.Models
{
    public abstract class Chat : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<SourceMessage> messages = new ObservableCollection<SourceMessage>();
        private int count;
        private bool canWrite;

        public int _messageCount = 15;
        public int _messageOffset = 0;
        public DateTime _offsetDate = DateTime.Now;

        private BitmapImage image;

        public BitmapImage Image { get => image; set => Set(ref image, value); }
        public bool CanWrite { get => canWrite; set => Set(ref canWrite, value); }

        private string userIsWriting;
        private Message lastMessage;

        protected Chat()
        {
            Messages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
               async delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (e.NewItems[0] is SystemMessage)
                            return;

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            SourceMessage sourceMessage = e.NewItems[0] as SourceMessage;
                            if (sourceMessage.Message is FileMessage)
                            {
                                FileMessage fileMessage = sourceMessage.Message as FileMessage;
                                string extension = fileMessage.FileName.Substring(fileMessage.FileName.LastIndexOf('.'));
                                switch (extension)
                                {
                                    case ".jpg":
                                    case ".png":
                                    case ".jpeg":
                                        if (sourceMessage is SessionSendedMessage)
                                        {
                                            sourceMessage.Message = LoadImageFromClient(fileMessage).Result;
                                            break;
                                        }
                                        sourceMessage.Message = LoadImageFromServer(fileMessage);
                                        break;
                                    default:
                                        if (sourceMessage is SessionSendedMessage)
                                        {
                                            UploadFileToServer(fileMessage);
                                        }
                                        fileMessage.IsLoaded = false;
                                        break;
                                }
                            }
                        });
                    }
                }
            );
        }

        protected Chat(IEnumerable<SourceMessage> messages) : this()
        {
            Messages = new ObservableCollection<SourceMessage>();
            foreach (var message in messages)
                Messages.Add(message);
        }

        protected Chat(IEnumerable<SourceMessage> messages, int count) : this(messages)
        {
            Count = count;
        }

        protected Chat(int sqlId) : this()
        {
            SqlId = sqlId;
        }

        private async System.Threading.Tasks.Task<ImageMessage> LoadImageFromClient(FileMessage fileMessage)
        {
            ImageMessage imageMessage = new ImageMessage(fileMessage.FileName, fileMessage.Date, fileMessage.StreamId);

            MemoryStream memoryStream = new MemoryStream();
            using (FileStream fileStream = new FileStream(fileMessage.FileName, FileMode.Open, FileAccess.Read))
            {
                ChatService.UploadFileInfo uploadFileInfo = new ChatService.UploadFileInfo();
                uploadFileInfo.FileName = fileMessage.FileName;
                uploadFileInfo.FileStream = fileStream;
                uploadFileInfo.Responsed_SqlId = ClientId;

                ChatService.FileClient fileClient = new ChatService.FileClient();
                fileStream.CopyTo(memoryStream);
                fileStream.Position = 0;
                await fileClient.FileUploadAsync(SqlId, uploadFileInfo.FileName, uploadFileInfo.Responsed_SqlId, uploadFileInfo.FileStream);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var bitmapImage = new BitmapImage();

                memoryStream.Seek(0, SeekOrigin.Begin);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.DecodePixelWidth = 400;
                bitmapImage.EndInit();

                imageMessage.Bitmap = bitmapImage;
            });
            imageMessage.FileName = imageMessage.FileName.Substring(imageMessage.FileName.LastIndexOf("\\") + 1);
            return imageMessage;
        }

        private void UploadFileToServer(FileMessage fileMessage)
        {

            using (FileStream fileStream = new FileStream(fileMessage.FileName, FileMode.Open, FileAccess.Read))
            {
                ChatService.UploadFileInfo uploadFileInfo = new ChatService.UploadFileInfo();
                uploadFileInfo.FileName = fileMessage.FileName;
                uploadFileInfo.FileStream = fileStream;
                uploadFileInfo.Responsed_SqlId = ClientId;

                ChatService.FileClient fileClient = new ChatService.FileClient();
                fileClient.FileUploadAsync(SqlId, uploadFileInfo.FileName, uploadFileInfo.Responsed_SqlId, uploadFileInfo.FileStream);
            }
        }

        private ImageMessage LoadImageFromServer(FileMessage fileMessage)
        {
            ImageMessage imageMessage = new ImageMessage(fileMessage.FileName, fileMessage.Date, fileMessage.StreamId, new BitmapImage());

            Stream stream = null;
            MemoryStream memoryStream = null;
            long lenght = 0;

            ChatService.FileClient fileClient = new ChatService.FileClient();
            try
            {
                string name = fileClient.FileDownload(fileMessage.StreamId, out lenght, out stream);
                if (lenght <= 0)
                    return null;
                memoryStream = Utility.FileHelper.ReadFileByPart(stream);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    memoryStream.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();

                    imageMessage.Bitmap = bitmapImage;
                });

                imageMessage.IsLoaded = false;
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            finally
            {
                if (memoryStream != null) memoryStream.Close();
                if (stream != null) stream.Close();
            }

            return imageMessage;
        }


        public int SqlId { get; set; }

        public int ClientId { get; set; }

        public ObservableCollection<SourceMessage> Messages { get => messages; set => Set(ref messages, value); }

        public int Count { get => count; set => Set(ref count, value); }

        public string UserIsWriting { get => userIsWriting; set => Set(ref userIsWriting, value); }

        public Message LastMessage { get => lastMessage; set => Set(ref lastMessage, value); }

        public virtual BitmapImage Avatar { set; get; }

        public abstract bool SetOnlineState(int userId, bool state);

        public abstract void MessageIsWriting(Nullable<int> userId);

        public abstract SourceMessage GetMessageType(int clientId, int senderId, Message message);

        public abstract void UserLeft(int userId);

        public abstract void RemoveUser(AvailableUser user);

        public abstract AvailableUser FindUser(Nullable<int> userId);


        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

        protected void Set(string prop_name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }


    public class ChatOne : Chat
    {
        private AvailableUser user;

        public ChatOne(int sqlId) : base(sqlId)
        {

        }

        public ChatOne(IEnumerable<SourceMessage> messages) : base(messages)
        {

        }

        public ChatOne(int sqlId, AvailableUser user) : this(sqlId)
        {
            User = user;
        }

        public ChatOne(IEnumerable<SourceMessage> messages, AvailableUser user, int count) : base(messages, count)
        {
            User = user;
        }

        public AvailableUser User { get => user; set => Set(ref user, value); }

        public override bool SetOnlineState(int userId, bool state)
        {
            if (user == null)
                return false;
            if (user.SqlId == userId)
            {
                user.IsOnline = state;
                return true;
            }
            return false;
        }

        public override void MessageIsWriting(Nullable<int> userId)
        {
            UserIsWriting = User.SqlId == userId ? User.Name + " is writing..." : null;
        }

        public override void UserLeft(int userId)
        {
            Messages.Add(SystemMessage.UserLeftChat(DateTime.Now, user.Name));
            user = null;
        }

        public override void RemoveUser(AvailableUser user)
        {
            if (this.user.SqlId == user.SqlId) this.user = null;
        }

        public override SourceMessage GetMessageType(int clientId, int senderId, Message message)
        {
            if (senderId == clientId)
                return new UserMessage(message);
            return new SourceMessage(message);
        }

        public override AvailableUser FindUser(int? userId)
        {
            return user;
        }
    }

    public class ChatGroup : Chat
    {
        private static string[] allColors;

        private string groupName;
        private ObservableCollection<AvailableUser> users = new ObservableCollection<AvailableUser>();
        private Dictionary<int, string> colors = new Dictionary<int, string>();

        static ChatGroup()
        {
            allColors = new string[13];
            allColors[0] = "#945b3a";
            allColors[1] = "#d95509";
            allColors[2] = "#d99b09";
            allColors[3] = "#6e8212";
            allColors[4] = "#51c404";
            allColors[5] = "#118515";
            allColors[6] = "#2f736c";
            allColors[7] = "#1978b3";
            allColors[8] = "#5c75ed";
            allColors[9] = "#7c1bc2";
            allColors[10] = "#c21bb7";
            allColors[11] = "#ed028b";
            allColors[12] = "#8c0315";
        }

        public ChatGroup(int sqlId, string groupName, IEnumerable<AvailableUser> users) : base(sqlId)
        {
            GroupName = groupName;
            foreach (var user in users)
            {
                Users.Add(user);
                colors.Add(user.SqlId, allColors[Users.Count % allColors.Length]);
            }
        }

        public string GroupName { get => groupName; set => Set(ref groupName, value); }

        public ObservableCollection<AvailableUser> Users { get => users; set => Set(ref users, value); }

        public void AddMember(AvailableUser user)
        {
            users.Add(user);
            colors.Add(user.SqlId, allColors[users.Count % allColors.Length]);
            Messages.Add(SystemMessage.UserAdded(DateTime.Now, user.Name));
        }

        public override bool SetOnlineState(int userId, bool state)
        {
            var user = FindUser(userId);
            if (user != null)
            {
                user.IsOnline = state;
                return true;
            }
            return false;
        }

        public override void MessageIsWriting(Nullable<int> userId)
        {
            var user = FindUser(userId);
            if (user == null)
            {
                UserIsWriting = null;
                return;
            }
            UserIsWriting = user.Name + " is writing...";
        }

        public override void UserLeft(int userId)
        {
            var user = FindUser(userId);
            if (user == null)
                return;
            Messages.Add(SystemMessage.UserLeftChat(DateTime.Now, user.Name));
            Users.Remove(user);
        }

        public override void RemoveUser(AvailableUser user)
        {
            if (Users.Remove(user))
                Messages.Add(SystemMessage.UserRemoved(DateTime.Now, user.Name));
        }

        public override AvailableUser FindUser(Nullable<int> userId)
        {
            if (userId == null)
                return null;
            foreach (var user in users)
                if (user.SqlId == userId)
                    return user;
            return null;
        }

        public AvailableUser GetUserById(int sqlId)
        {
            foreach (var user in users)
            {
                if (user.SqlId == sqlId) return user;
            }
            return null;
        }

        public override SourceMessage GetMessageType(int clientId, int senderId, Message message)
        {
            if (senderId == clientId)
                return new UserMessage(message);
            var user = GetUserById(senderId);
            if (user != null)
                return new GroupMessage(message, GetUserById(senderId), colors[senderId]);
            else
                return new GroupMessage(message, new ChatService.UnitClient().FindUserName(senderId), "red");
        }
    }
}