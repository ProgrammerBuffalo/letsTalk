using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Client
{
    // Данные пользователя
    public class ClientUserInfo : INotifyPropertyChanged
    {
        private BitmapImage userImage = null; // Аватарка

        public ChatService.ChatClient ChatClient { private set; get; } // Сеанс

        public int SqlId { private set; get; } // Id в БД

        public Guid ConnectionId { private set; get; } // Сеансовый Id

        public string UserName { get; set; } // Никнейм (Логин == Никнейм)

        public BitmapImage UserImage
        {
            get { return userImage; }
            set { userImage = value; OnPropertyChanged("UserImage"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propName));
        }

        public ClientUserInfo(Guid unique_id, int sqlId, ChatService.ChatClient chatClient, string userName)
        {
            ConnectionId = unique_id;
            SqlId = sqlId;
            this.ChatClient = chatClient;
            UserName = userName;
        }

    }
}
