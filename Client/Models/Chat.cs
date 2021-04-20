using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        ClientUserInfo clientUserInfo;

        public ChatOne()
        {

        }

        public ChatOne(IEnumerable<SourceMessage> messages) : base(messages)
        {

        }

        public ChatOne(IEnumerable<SourceMessage> messages, ClientUserInfo user, int count) : base(messages, count)
        {
            ClientUserInfo = user;
        }

        public ClientUserInfo ClientUserInfo { get => clientUserInfo; set => Set(ref clientUserInfo, value); }
    }

    public class ChatGroup : Chat
    {
        private string groupName;
        private string groupDesc;
        private ObservableCollection<ClientUserInfo> users;

        public ChatGroup()
        {

        }

        public ChatGroup(string groupName, string groupDesc)
        {
            GroupName = groupName;
            GroupDesc = groupDesc;
        }

        public string GroupName { get => groupName; set => Set(ref groupName, value); }
        public string GroupDesc { get => groupDesc; set => Set(ref groupDesc, value); }
        public ObservableCollection<ClientUserInfo> Users { get => users; set => Set(ref users, value); }
    }
}
