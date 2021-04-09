using Client.Models;
using Client.Utility;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Элементы Гамбургер меню
        private ObservableCollection<HamburgerMenuIconItem> optionViewList;

        private ContentControl currentView;

        private ClientUserInfo clientUserInfo;

        public MainViewModel()
        {
            LoadedWindowCommand = new Command(LoadedWindow);
            ClosedWindowCommand = new Command(ClosedWindow);
            SelectedHambugerItemOptionCommand = new Command(SelectedHambugerItemOption);
            LoadBurgerMenu();
        }

        public MainViewModel(string name, int sqlId) : this()
        {
            ChatService.ChatClient chatClient = new ChatService.ChatClient();
            Guid unique_id = chatClient.Connect(sqlId);

            ClientUserInfo = new ClientUserInfo(unique_id, sqlId, chatClient, name);
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedHambugerItemOptionCommand { get; }

        public ObservableCollection<HamburgerMenuIconItem> OptionViewList { get => optionViewList; set => Set(ref optionViewList, value); }

        public ClientUserInfo ClientUserInfo { get => clientUserInfo; set => Set(ref clientUserInfo, value); } // Вся информация о подключенном юзере

        public ContentControl CurrentView { get => currentView; set => Set(ref currentView, value); }

        public void ClosedWindow(object sender)
        {
            ClientUserInfo.ChatClient.Disconnect(ClientUserInfo.ConnectionId);
        }

        public void SelectedHambugerItemOption(object sender)
        {
            HamburgerMenuIconItem menuIconItem = sender as HamburgerMenuIconItem;
            Type userControl = Type.GetType("Client.Views." + menuIconItem.Tag);

            CurrentView = Activator.CreateInstance(userControl) as ContentControl;

            //что это?
            ContentControl contentControl = new ContentControl();
        }

        // После того как окно полностью прогрузилось, у нас происходит вызов загрузки аватарки с сервера к пользователю
        public void LoadedWindow(object sender)
        {
            try
            {
                clientUserInfo.DownloadAvatarAsync();
            }
            catch (FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (IOException)
            {
                MessageBox.Show("avatar image could not be download");
            }
        }

        private void LoadBurgerMenu()
        {
            OptionViewList = new ObservableCollection<HamburgerMenuIconItem>();

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

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
