namespace Client.Models
{
    public enum MessageState { Readen, Recived, Send, NoInet };

    // тут храниться сообшение которые пришли к клиенту
    public class SourceMessage : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private Message message;

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
    }

    // тут храниться сообшение которые послал клиент
    public class UserMessage : SourceMessage
    {
        private MessageState state;

        public UserMessage()
        {

        }

        public UserMessage(Message message) : base(message)
        {

        }

        public MessageState State { get => state; set => Set(ref state, value); }
    }

    // тут храниться сообшение от группы
    // в будушим сдесь будут поля которые будут показывать какие пользователи прочитали, приняли, не приняли сообщение
    public class GroupMessage : SourceMessage
    {
        //поверх сообщения будет видно какой именно член группы отправил сообшение
        private AvailableUser user;
        private string color;

        public GroupMessage()
        {

        }

        public GroupMessage(Message message) : base(message)
        {

        }

        public GroupMessage(Message message, AvailableUser user) : base(message)
        {
            User = user;
        }

        public AvailableUser User { get => user; set => Set(ref user, value); }
        public string Color { get => color; set => Set(ref color, value); }
    }

    //системнные сообшение что типо пользователь покинул чат или кто стал админом чата
    //скорее тут будут static поля с систменными сообшения 
    public class SystemMessage : SourceMessage
    {
        public SystemMessage()
        {

        }

        public SystemMessage(Message message) : base(message)
        {

        }

        public static SystemMessage UserLeavedChat(string login)
        {
            return new SystemMessage(new TextMessage(login + " leaved chat :("));
        }

        public static SystemMessage UserBecomeAdmin(string admin, string login)
        {
            return new SystemMessage(new TextMessage(admin + "gaved admin to" + login));
        }

        public static SystemMessage UserRemoved(string login)
        {
            return new SystemMessage(new TextMessage(login + " was removed from chat"));
        }

        public static SystemMessage UserAdded(string login)
        {
            return new SystemMessage(new TextMessage(login + "added to chat"));
        }
    }
}
