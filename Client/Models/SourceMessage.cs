using System;

namespace Client.Models
{
    public class SourceMessage : System.ComponentModel.INotifyPropertyChanged, ICloneable
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected Message message;

        public SourceMessage()
        {

        }

        public SourceMessage(Message message)
        {
            Message = message;
        }

        public Message Message { get => message; set => Set(ref message, value); }

        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

        public virtual object Clone()
        {
            return new SourceMessage((Message)message.Clone());
        }
    }

    public class SessionSendedMessage : SourceMessage
    {
        public SessionSendedMessage(Message message) : base(message)
        {

        }

        public override object Clone()
        {
            return new SessionSendedMessage((Message)message.Clone());
        }
    }

    public class UserMessage : SourceMessage
    {
        public UserMessage()
        {

        }

        public UserMessage(Message message) : base(message)
        {

        }

        public override object Clone()
        {
            return new UserMessage((Message)message.Clone());
        }
    }

    public class GroupMessage : SourceMessage
    {
        private AvailableUser user;
        private string color;

        public GroupMessage()
        {

        }

        public GroupMessage(Message message) : base(message)
        {

        }

        public GroupMessage(Message message, string userName, string color) : base(message)
        {
            user = new AvailableUser(0, userName);
            this.color = color;
        }

        public GroupMessage(Message message, AvailableUser user, string color) : base(message)
        {
            User = user;
            Color = color;
        }

        public AvailableUser User { get => user; set => Set(ref user, value); }
        public string Color { get => color; set => Set(ref color, value); }

        public override object Clone()
        {
            return new GroupMessage((Message)message.Clone(), (AvailableUser)user.Clone(), color);
        }
    }

    public class SystemMessage : SourceMessage
    {
        public SystemMessage(Message message)
        {
            this.message = message;
        }

        public static SystemMessage ShiftDate(DateTime dateTime)
        {
            return new SystemMessage(new TextMessage(dateTime.ToShortDateString()));
        }

        public static SystemMessage UserLeftChat(DateTime dateTime, string nickname)
        {
            return new SystemMessage(new TextMessage(nickname + " left chat :(" + "   " + dateTime.ToShortTimeString(), dateTime));
        }

        public static SystemMessage UserRemoved(DateTime dateTime, string nickname)
        {
            return new SystemMessage(new TextMessage(nickname + " was removed from chat" + "   " + dateTime.ToShortTimeString(), dateTime));
        }

        public static SystemMessage UserAdded(DateTime dateTime, string nickname)
        {
            return new SystemMessage(new TextMessage(nickname + " was added to chat" + "   " + dateTime.ToShortTimeString(), dateTime));
        }

        public static SystemMessage ChatroomCreated(DateTime dateTime)
        {
            return new SystemMessage(new TextMessage("Chatroom was created " + dateTime.ToShortTimeString(), dateTime));
        }

        public override object Clone()
        {
            return new SystemMessage((Message)message.Clone());
        }
    }
}
