using System.ComponentModel;

namespace Client.Models
{
    class NotifyMessage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Chat chat;
        private Message notify;

        public NotifyMessage(Chat chat, Message notify)
        {
            this.chat = chat;
            this.notify = notify;
        }

        public Chat Chat { get => chat; set => Set(ref chat, value); }
        public Message Notify { get => notify; set => Set(ref notify, value); }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
