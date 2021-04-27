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

        private string userIsWriting;

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
                                        if(sourceMessage is SessionSendedMessage)
                                        {
                                            sourceMessage.Message = LoadImageFromClient(fileMessage);
                                            break;
                                        }
                                        sourceMessage.Message = LoadImageFromServer(fileMessage);
                                        break;
                                    default:
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

        private ImageMessage LoadImageFromClient(FileMessage fileMessage)
        {
            ImageMessage imageMessage = new ImageMessage(fileMessage.FileName, fileMessage.Date, fileMessage.StreamId);

            MemoryStream memoryStream = new MemoryStream();
            using(FileStream fileStream = new FileStream(fileMessage.FileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                imageMessage.Bitmap = bitmapImage;
            });
            imageMessage.FileName = imageMessage.FileName.Substring(imageMessage.FileName.LastIndexOf("\\") + 1);
            imageMessage.IsLoaded = false;
            return imageMessage;
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

        public virtual async void DownloadAvatarAsync() { }

        public int SqlId { get; set; }

        public ObservableCollection<SourceMessage> Messages { get => messages; set => Set(ref messages, value); }

        public int Count { get => count; set => Set(ref count, value); }


        //количество не прочитанных смс
        public string UserIsWriting { get => userIsWriting; set => Set(ref userIsWriting, value); }

        public virtual BitmapImage Avatar { set; get; }

        public abstract bool SetOnlineState(int userId, bool state);

        public abstract void MessageIsWriting(Nullable<int> userId);

        public abstract SourceMessage GetMessageType(int senderId, Message message);

        public abstract void UserLeavedChatroom(int userId);

        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }


    public class ChatOne : Chat
    {
        AvailableUser user;

        //убрать
        public ChatOne()
        {
            user = new AvailableUser();
        }

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

        public override BitmapImage Avatar
        {
            get => user.Image;
            set
            {
                var image = user.Image;
                user.Image = value;
                Set(ref image, value);
            }
        }

        public override bool SetOnlineState(int userId, bool state)
        {
            if (user.SqlId == userId)
            {
                user.IsOnline = state;
                return true;
            }
            return false;
        }

        public override void MessageIsWriting(Nullable<int> userId)
        {
            UserIsWriting = User.SqlId == userId ? User.Name + " is writing..." : "";
        }

        public override void UserLeavedChatroom(int userId)
        {
            Messages.Add(SystemMessage.UserLeavedChat(user.Name));
        }

        public override async void DownloadAvatarAsync()
        {
            ChatService.DownloadRequest request = new ChatService.DownloadRequest(SqlId);
            var avatarClient = new ChatService.AvatarClient();

            Stream stream = null;
            long lenght = 0;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    avatarClient.UserAvatarDownload(user.SqlId, out lenght, out stream);
                    if (lenght <= 0)
                        return;
                    MemoryStream memoryStream = Utility.FileHelper.ReadFileByPart(stream);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();

                        Avatar = bitmapImage;
                    });
                    memoryStream.Close();
                    memoryStream.Dispose();
                    stream.Close();
                    stream.Dispose();
                });

            }
            catch (System.ServiceModel.FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                throw ex;
            }
        }

        public override SourceMessage GetMessageType(int senderId, Message message)
        {
            if (senderId == ClientUserInfo.getInstance().SqlId)
                return new UserMessage(message);
            return new SourceMessage(message);
        }
    }

    public class ChatGroup : Chat
    {
        private static string[] allColors;

        private string groupName;
        private string groupDesc;
        private ObservableCollection<AvailableUser> users = new ObservableCollection<AvailableUser>();
        private Dictionary<AvailableUser, string> colors;

        private BitmapImage image;

        public BitmapImage Image { get => image; set => Set(ref image, value); }

        static ChatGroup()
        {
            allColors = new string[20];
            allColors[0] = "Blue";
            allColors[1] = "Green";
            allColors[2] = "Red";
        }

        //убрать
        public ChatGroup()
        {

        }

        public ChatGroup(int sqlId, string groupName, IEnumerable<AvailableUser> users) : base(sqlId)
        {
            GroupName = groupName;
            foreach (var user in users)
                Users.Add(user);
        }

        public ChatGroup(int sqlId, string groupName, string groupDesc, IEnumerable<AvailableUser> users) : this(sqlId, groupName, users)
        {
            GroupDesc = groupDesc;
        }

        public string GroupName { get => groupName; set => Set(ref groupName, value); }
        public string GroupDesc { get => groupDesc; set => Set(ref groupDesc, value); }

        public ObservableCollection<AvailableUser> Users { get => users; set => Set(ref users, value); }

        public void AddMember(AvailableUser user)
        {
            users.Add(user);
            Messages.Add(SystemMessage.UserAdded(user.Name));
        }

        public void RemoveMember(int userId)
        {
            var user = FindUser(userId);
            users.Remove(user);
            Messages.Add(SystemMessage.UserRemoved(user.Name));
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
                UserIsWriting = "";
                return;
            }
            UserIsWriting = user.Name + " is writing...";
        }

        public override void UserLeavedChatroom(int userId)
        {
            var user = FindUser(userId);
            Messages.Add(SystemMessage.UserLeavedChat(user.Name));
        }

        private AvailableUser FindUser(Nullable<int> userId)
        {
            if (userId == null)
                return null;
            foreach (var user in users)
                if (user.SqlId == userId)
                    return user;
            return null;
        }

        public override async void DownloadAvatarAsync()
        {
            ChatService.DownloadRequest request = new ChatService.DownloadRequest(SqlId);
            var avatarClient = new ChatService.AvatarClient();

            Stream stream = null;
            long lenght = 0;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    avatarClient.ChatAvatarDownload(SqlId, out lenght, out stream);
                    if (lenght <= 0)
                        return;
                    MemoryStream memoryStream = Utility.FileHelper.ReadFileByPart(stream);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();

                        Image = bitmapImage;
                    });
                    memoryStream.Close();
                    memoryStream.Dispose();
                    stream.Close();
                    stream.Dispose();
                });

            }
            catch (System.ServiceModel.FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                throw ex;
            }
        }

        public override SourceMessage GetMessageType(int senderId, Message message)
        {
            if (senderId == ClientUserInfo.getInstance().SqlId)
                return new GroupMessage(message);
            return new SourceMessage(message);
        }
    }
}