using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class SettingsViewModel : System.ComponentModel.INotifyPropertyChanged, IHelperUC
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event UCChangedEventHandler RemoveUC;
        public event UCChangedEventHandler AddUC;

        private Settings settings;
        private System.Windows.Controls.UserControl selectedSubSetting;
        private ObservableCollection<Rington> userRingtons;
        private ObservableCollection<Rington> groupRingtons;

        private Rington selectedUserRington;
        private Rington selectedGroupRington;

        private Views.DefaultWallpaperWindow wallpaperWindow;
        private string selectedWallpaper;
        private Rington selectedRington;


        public SettingsViewModel(ClientUserInfo client)
        {
            Client = client;
            settings = Settings.Instance;
            DefautWallpapers = new ObservableCollection<string>(settings.GetWallpapers());
            UserRingtons = new ObservableCollection<Rington>(settings.GetRingtons());
            GroupRingtons = new ObservableCollection<Rington>(settings.GetRingtons());
            for (int i = 0; i < userRingtons.Count; i++)
            {
                if (userRingtons[i].Name == settings.SelectedUserRington.Name)
                    userRingtons[i].IsSelected = true;
            }
            for (int i = 0; i < groupRingtons.Count; i++)
            {
                if (groupRingtons[i].Name == settings.SelectedGroupRington.Name)
                    groupRingtons[i].IsSelected = true;
            }

            WindowLoadedCommand = new Command(WindowLoaded);
            GeneralSettingsShowCommand = new Command(GeneralSettingsShow);
            AppearanceSettingsShowCommand = new Command(AppearanceSettingsShow);
            NotificationSettingsShowCommand = new Command(NotificationSettingsShow);
            DefaultWallpaperShowCommand = new Command(DefaultWallpaperShow);
            DeviceWallpaperShowCommand = new Command(DeviceWallpaperShow);
            DefaultWallpaperChangedCommand = new Command(DefaultWallpaperChanged);
            UserRingtonChangedCommand = new Command(UserRingtonChanged);
            GroupRingtonChangedCommand = new Command(GroupRingtonChanged);
            LanguageChangedCommand = new Command(LanguageChanged);

            Messages = new ObservableCollection<SourceMessage>();
            Messages.Add(new SourceMessage(new TextMessage("Hello")));
            Messages.Add(new UserMessage(new TextMessage("Hi how are you")));
            Messages.Add(new SourceMessage(new TextMessage("I am okey)")));
            Messages.Add(new SourceMessage(new TextMessage("thank you bro")));
            Messages.Add(new GroupMessage(new TextMessage("hello guys lets have some fun!!!"), new AvailableUser() { Name = "John", IsOnline = true, Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("Resources/group.png", UriKind.Relative)) }, "#111111"));
            Messages.Add(SystemMessage.ChatroomCreated(DateTime.Now));

            selectedSubSetting = new Views.GeneralSettingsUC();
            selectedSubSetting.DataContext = this;
        }

        public ICommand WindowLoadedCommand { get; }
        public ICommand DefaultWallpaperShowCommand { get; }
        public ICommand DeviceWallpaperShowCommand { get; }
        public ICommand DefaultWallpaperChangedCommand { get; }
        public ICommand UserRingtonChangedCommand { get; }
        public ICommand GroupRingtonChangedCommand { get; }
        public ICommand LanguageChangedCommand { get; }
        public ICommand GeneralSettingsShowCommand { get; }
        public ICommand AppearanceSettingsShowCommand { get; }
        public ICommand NotificationSettingsShowCommand { get; }


        public ClientUserInfo Client { get; }
        public Settings Settings { get => settings; set => Set(ref settings, value); }
        public ObservableCollection<string> DefautWallpapers { get; private set; }
        public ObservableCollection<SourceMessage> Messages { get; }
        public ObservableCollection<Rington> UserRingtons { get => userRingtons; private set => userRingtons = value; }
        public ObservableCollection<Rington> GroupRingtons { get => groupRingtons; private set => groupRingtons = value; }
        public Rington SelectedUserRington { get => selectedUserRington; set => selectedUserRington = value; }
        public Rington SelectedGroupRington { get => selectedGroupRington; set => selectedGroupRington = value; }
        public string SelectedWallpaper { get => selectedWallpaper; set => Set(ref selectedWallpaper, value); }
        public Rington SelectedRington { get => selectedRington; set => Set(ref selectedRington, value); }

        private void WindowLoaded(object param)
        {
            AddUC.Invoke(selectedSubSetting);
        }

        private void GeneralSettingsShow(object param)
        {
            if (!(selectedSubSetting is Views.GeneralSettingsUC))
            {
                System.Windows.Controls.UserControl subSetting = new Views.GeneralSettingsUC();
                RemoveUC(selectedSubSetting);
                selectedSubSetting = subSetting;
                selectedSubSetting.DataContext = this;
                AddUC(selectedSubSetting);
            }
        }

        private void AppearanceSettingsShow(object param)
        {
            if (!(selectedSubSetting is Views.ApirianceSettigsUC))
            {
                System.Windows.Controls.UserControl subSetting = new Views.ApirianceSettigsUC();
                RemoveUC(selectedSubSetting);
                selectedSubSetting = subSetting;
                selectedSubSetting.DataContext = this;
                AddUC(selectedSubSetting);
            }
        }

        private void NotificationSettingsShow(object param)
        {
            if (!(selectedSubSetting is Views.NotificationSettingsUC))
            {
                System.Windows.Controls.UserControl subSetting = new Views.NotificationSettingsUC();
                RemoveUC(selectedSubSetting);
                selectedSubSetting = subSetting;
                selectedSubSetting.DataContext = this;
                AddUC(selectedSubSetting);
            }
        }

        private void LanguageChanged(object param)
        {
            settings.SelectedLanguage = param.ToString();
        }

        private void DefaultWallpaperShow(object param)
        {
            wallpaperWindow = new Views.DefaultWallpaperWindow();
            wallpaperWindow.DataContext = this;
            wallpaperWindow.ShowDialog();
        }

        private void DefaultWallpaperChanged(object param)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            settings.DeleteFromDeviceWallpaper();
            wallpaperWindow.Close();
        }

        private void DeviceWallpaperShow(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = App.Current.Resources["ImageFiles"].ToString();
            if (dialog.ShowDialog() == true)
            {
                settings.SelectedWallpaper = "";
                GC.Collect();
                GC.WaitForPendingFinalizers();
                settings.DeleteFromDeviceWallpaper();
                string path = AppDomain.CurrentDomain.BaseDirectory + settings.WallaperFolderPath + "\\" + dialog.FileName.Substring(dialog.FileName.LastIndexOf('\\') + 1);
                File.Copy(dialog.FileName, path);
                Settings.SelectedWallpaper = path;
            }
        }

        private void UserRingtonChanged(object param)
        {
            if (param != null)
            {
                Rington rington = (Rington)param;
                Settings.SelectedUserRington = rington;
                settings.PlayRington(Settings.SelectedUserRington.Path);
            }
        }

        private void GroupRingtonChanged(object param)
        {
            if (param != null)
            {
                Rington rington = (Rington)param;
                Settings.SelectedGroupRington = rington;
                settings.PlayRington(Settings.SelectedGroupRington.Path);
            }
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
