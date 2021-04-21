using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Client.Models
{
    // сюда идет реализация IChatCallback
    // если надо сделай эти методы виртуальными
    public class Chat : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<SourceMessage> messages;
        private int count;

        public Chat()
        {

        }

        public Chat(IEnumerable<SourceMessage> messages)
        {
            Messages = new ObservableCollection<SourceMessage>();
            foreach (var message in messages)
                Messages.Add(message);
        }

        public Chat(IEnumerable<SourceMessage> messages, int count) : this(messages)
        {
            Count = count;
        }

        public Chat(int sqlId)
        {
            SqlId = sqlId;
        }

        public int SqlId { get; set; }

        public ObservableCollection<SourceMessage> Messages { get => messages; set => Set(ref messages, value); }

        //количество не прочитанных смс
        public int Count { get => count; set => Set(ref count, value); }

        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }

    public class ChatOne : Chat
    {
        AvailableUser user;

        public ChatOne()
        {

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
    }

    public class ChatGroup : Chat
    {
        private string groupName;
        private string groupDesc;
        private BitmapImage image;
        private ObservableCollection<AvailableUser> users;

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
        public BitmapImage Image { get => image; set => Set(ref image, value); }
        public ObservableCollection<AvailableUser> Users { get => users; set => Set(ref users, value); }
    }
}
