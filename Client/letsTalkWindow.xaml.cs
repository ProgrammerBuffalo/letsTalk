using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{

    public partial class letsTalkWindow : Window, INotifyPropertyChanged
    {
        private ChatService.ChatClient chatClient;

        private BitmapImage userImage = null;

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

        public string UserName { get; set; }

        public Guid ConnectionId { get; set; }

        public int SqlId { get; set; }

        public letsTalkWindow()
        {
            InitializeComponent();
        }

        public letsTalkWindow(string name, int Id) : this()
        {
            UserName = name;
            SqlId = Id;
            hamburgerMenu.ItemsSource = this.DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                chatClient = new ChatService.ChatClient();
                ConnectionId = chatClient.Connect(SqlId);

                DownloadAvatarAsync(new ChatService.DownloadRequest(SqlId));
            }
            catch (FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void DownloadAvatarAsync(ChatService.DownloadRequest request)
        {
            var fileClient = new Client.ChatService.FileClient();

            Stream stream = null;
            long lenght;

            try
            {

                await Task.Factory.StartNew
                   (() =>
                   {
                       fileClient.AvatarDownload(request.Requested_UserSqlId, out lenght, out stream);
                       MemoryStream memoryStream = new MemoryStream();

                       const int bufferSize = 2048;
                       var buffer = new byte[bufferSize];

                       do
                       {
                           int bytesRead = stream.Read(buffer, 0, bufferSize);

                           if (bytesRead == 0) { break; }

                           memoryStream.Write(buffer, 0, bytesRead);
                       } while (true);

                       memoryStream.Seek(0, SeekOrigin.Begin);

                       Dispatcher.Invoke(() =>
                       {
                           BitmapImage bitmapImage = new BitmapImage();

                           bitmapImage.BeginInit();
                           bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                           bitmapImage.StreamSource = memoryStream;
                           bitmapImage.EndInit();

                           UserImage = bitmapImage;

                       });

                       memoryStream.Close();
                   });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { if (stream != null) stream.Dispose(); }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Settings!");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            chatClient.Disconnect(ConnectionId);
        }
    }
}
