using Client.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows.Media.Imaging;

namespace Client.Models
{
    public enum Activity { Online, Offline, Busy }

    public class ClientUserInfo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private static ClientUserInfo instance;

        private string userName;
        private string userDesc; // описание пользователя (Hey there i am using Lets Talk!!!)
        private BitmapImage userImage = null; // Аватарка
        private Activity activity; // поле для показа подключенных и не подключенных клиентов


        public static ClientUserInfo getInstance()
        {
            return instance;
        }

        public ClientUserInfo(string userName, string userDesc, string imagePath, Activity activity)
        {
            UserName = userName;
            UserDesc = userDesc;
            UserImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + imagePath));
            Activity = activity;
        }

        public ClientUserInfo(Dictionary<int, int[]> clients, int sqlId, ChatService.ChatClient chatClient, string userName)
        {
            Clients = clients;
            SqlId = sqlId;
            ChatClient = chatClient;
            UserName = userName;
            instance = this;
        }

        public ChatService.ChatClient ChatClient { private set; get; } // Сеанс

        public int SqlId { private set; get; } // Id в БД

        public Dictionary<int, int[]> Clients { get; set; }

        public string UserName { get => userName; set => Set(ref userName, value); }

        public string UserDesc { get => userDesc; set => Set(ref userDesc, value); }

        public Activity Activity { get => activity; set => Set(ref activity, value); }

        public BitmapImage UserImage { get => userImage; set => Set(ref userImage, value); }

        public async void DownloadAvatarAsync()
        {
            ChatService.DownloadRequest request = new ChatService.DownloadRequest(SqlId);
            var avatarClient = new ChatService.AvatarClient();

            Stream stream = null;
            long lenght = 0;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                 {
                     avatarClient.AvatarDownload(SqlId, out lenght, out stream);
                     MemoryStream memoryStream = FileHelper.ReadFileByPart(stream);

                     System.Windows.Application.Current.Dispatcher.Invoke(() =>
                     {
                         var bitmapImage = new BitmapImage();
                         bitmapImage.BeginInit();
                         bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                         bitmapImage.StreamSource = memoryStream;
                         bitmapImage.EndInit();

                         UserImage = bitmapImage;
                     });
                     memoryStream.Close();
                     memoryStream.Dispose();
                     stream.Close();
                     stream.Dispose();
                 });

            }
            catch (FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                throw ex;
            }
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }

}
