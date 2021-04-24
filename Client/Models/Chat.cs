using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace Client.Models
{
    public abstract class Chat : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<SourceMessage> messages;
        private int count;
        private bool isWriting;
        private string isWritingName;

        protected Chat()
        {

        }

        protected Chat(IEnumerable<SourceMessage> messages)
        {
            Messages = new ObservableCollection<SourceMessage>();
            foreach (var message in messages)
                Messages.Add(message);
        }

        protected Chat(IEnumerable<SourceMessage> messages, int count) : this(messages)
        {
            Count = count;
        }

        protected Chat(int sqlId)
        {
            SqlId = sqlId;
        }

        public virtual async void DownloadAvatarAsync() { }

        public int SqlId { get; set; }

        public ObservableCollection<SourceMessage> Messages { get => messages; set => Set(ref messages, value); }

        //количество не прочитанных смс
        public int Count { get => count; set => Set(ref count, value); }

        public bool IsWriting { get => isWriting; set => Set(ref isWriting, value); }

        public string IsWritingName { get => isWritingName; set => Set(ref isWritingName, value); }

        public virtual BitmapImage Avatar { set; get; }

        public abstract void SetOnlineState(int userId, bool state);

        public abstract void MessageIsWriting(int userId, bool state);

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

        public ChatOne(int sqlId, AvailableUser user)
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

        public override void SetOnlineState(int userId, bool state)
        {
            if (user.SqlId == userId) user.IsOnline = true;
        }

        public override void MessageIsWriting(int userId, bool state)
        {
            IsWriting = state;
            IsWritingName = user.Name;
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

                        user.Image = bitmapImage;
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

    }

    public class ChatGroup : Chat
    {
        private static string[] allColors;

        private string groupName;
        private string groupDesc;
        private ObservableCollection<AvailableUser> users;
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
            users = new ObservableCollection<AvailableUser>();
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

        public override void SetOnlineState(int userId, bool state)
        {
            var user = FindUser(userId);
            user.IsOnline = state;
        }

        public override void MessageIsWriting(int userId, bool state)
        {
            var user = FindUser(userId);
            IsWriting = state;
            IsWritingName = user.Name;
        }

        public override void UserLeavedChatroom(int userId)
        {
            var user = FindUser(userId);
            Messages.Add(SystemMessage.UserLeavedChat(user.Name));
        }

        private AvailableUser FindUser(int userId)
        {
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
    }
}
