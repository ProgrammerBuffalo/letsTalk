using Client.Views;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows.Controls;

namespace Client
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Элементы Гамбургер меню
        private ObservableCollection<HamburgerMenuIconItem> _optionViewList;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<HamburgerMenuIconItem> OptionViewList
        {
            get { return _optionViewList; }
            set
            {
                _optionViewList = value;
                RaisePropertyChanged("OptionViewList");
            }
        }

        public ClientUserInfo clientUserInfo { get; set; } // Вся информация о подключенном юзере

        private ContentControl _currentView;

        public ContentControl CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedHambugerItemOptionCommand { get; }

        public MainViewModel()
        {
            LoadedWindowCommand = new Command(LoadedWindow);
            ClosedWindowCommand = new Command(ClosedWindow);
            SelectedHambugerItemOptionCommand = new Command(SelectedHambugerItemOption);

            _optionViewList = new ObservableCollection<HamburgerMenuIconItem>();

            OptionViewList.Add(new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial()
                {
                    Kind = PackIconMaterialKind.AccountHeart,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 22,
                    Height = 22
                },
                Label = "Available Users",
                Tag = "UCAvailableUsers"
            });

            OptionViewList.Add(new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial()
                {
                    Kind = PackIconMaterialKind.Shield,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 22,
                    Height = 22
                },
                Label = "Settings",
                Tag = "UCSettings"
            });
        }

        public MainViewModel(string name, int sqlId) : this()
        {
            ChatService.ChatClient chatClient = new ChatService.ChatClient();
            Guid unique_id = chatClient.Connect(sqlId);

            clientUserInfo = new ClientUserInfo(unique_id, sqlId, chatClient, name);
        }

        public void ClosedWindow(object sender)
        {
            clientUserInfo.ChatClient.Disconnect(clientUserInfo.ConnectionId);
        }

        public void SelectedHambugerItemOption(object sender)
        {
            HamburgerMenuIconItem menuIconItem = sender as HamburgerMenuIconItem;            
            Type userControl = Type.GetType("Client.Views." + menuIconItem.Tag);

            CurrentView = Activator.CreateInstance(userControl) as ContentControl;
            ContentControl contentControl = new ContentControl();
        }

        // После того как окно полностью прогрузилось, у нас происходит вызов загрузки аватарки с сервера к пользователю
        public void LoadedWindow(object sender)
        {
            try
            {
                DownloadAvatarAsync(new ChatService.DownloadRequest(clientUserInfo.SqlId));
            }
            catch (FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Сегментами подгружаем аватарку
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

                        System.Windows.Application.Current.Dispatcher.Invoke(
                        (Action)(() =>
                        {
                            BitmapImage bitmapImage = new BitmapImage();

                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();

                            clientUserInfo.UserImage = bitmapImage;
                        }));

                     

                       memoryStream.Close();
                   });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { if (stream != null) stream.Dispose(); }
        }

    }
}
