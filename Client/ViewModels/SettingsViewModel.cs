using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class SettingsViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private Settings settings;
        
        private Views.DefaultWallpaperWindow wallpaperWindow;
        private Views.PreviewWallpaperWindow previewWallpaperWindow;
        private string selectedWallpaper;
        
        private bool loadCount1IsChecked;
        private bool loadCount2IsChecked;
        private bool loadCount3IsChecked;
        private bool isFromDevice;


        public SettingsViewModel(ClientUserInfo client)
        {
            Client = client;
            settings = Settings.Instance;

            MessageLoadCountChangedCommand = new Command(MessageLoadCountChanged);
            DefaultWallpaperShowCommand = new Command(DefaultWallpaperShow);
            DeviceWallpaperShowCommand = new Command(DeviceWallpaperShow);
            DefaultWallpaperChangedCommand = new Command(DefaultWallpaperChanged);
            PreviewWallpaperShowCommand = new Command(PreviewWallpaperShow);
            ConfirmWallpaperCommand = new Command(ConfirmWallpaper);
            DefaultRingtonShowCommand = new Command(DefaultRingtonShow);
            UserRingtonChangeCommand = new Command(UserRingtonShow);
            GroupRingtonChangeCommand = new Command(GroupRingtonShow);

            Messages = new ObservableCollection<SourceMessage>();
            Messages.Add(new SourceMessage(new TextMessage("Hello")));
            Messages.Add(new UserMessage(new TextMessage("Hi how are you")));
            Messages.Add(new SourceMessage(new TextMessage("I am okey thanks ;)")));

            if (settings.MessageLoadCount == 30) LoadCount1IsChecked = true;
            else if (settings.MessageLoadCount == 50) LoadCount2IsChecked = true;
            else LoadCount3IsChecked = true;
        }

        public ICommand MessageLoadCountChangedCommand { get; }
        public ICommand DefaultWallpaperShowCommand { get; }
        public ICommand DeviceWallpaperShowCommand { get; }
        public ICommand DefaultWallpaperChangedCommand { get; }
        public ICommand DefaultRingtonShowCommand { get; }
        public ICommand DeviceRingtonShowCommand { get; }
        public ICommand ConfirmWallpaperCommand { get; }
        public ICommand PreviewWallpaperShowCommand { get; }
        public ICommand UserRingtonChangeCommand { get; }
        public ICommand GroupRingtonChangeCommand { get; }


        public ClientUserInfo Client { get; }
        public Settings Settings { get => settings; set => Set(ref settings, value); }
        public ObservableCollection<string> DefautWallpapers { get; private set; }
        public ObservableCollection<SourceMessage> Messages { get; }
        public string SelectedWallpaper { get => selectedWallpaper; set => Set(ref selectedWallpaper, value); }

        public bool LoadCount1IsChecked { get => loadCount1IsChecked; set => Set(ref loadCount1IsChecked, value); }
        public bool LoadCount2IsChecked { get => loadCount2IsChecked; set => Set(ref loadCount2IsChecked, value); }
        public bool LoadCount3IsChecked { get => loadCount3IsChecked; set => Set(ref loadCount3IsChecked, value); }

        private void MessageLoadCountChanged(object param)
        {
            settings.MessageLoadCount = int.Parse(param.ToString());
        }

        private void DefaultWallpaperShow(object param)
        {
            isFromDevice = false;
            DefautWallpapers = new ObservableCollection<string>(settings.GetWallpapers());
            wallpaperWindow = new Views.DefaultWallpaperWindow();
            wallpaperWindow.DataContext = this;
            wallpaperWindow.ShowDialog();
        }

        private void DefaultWallpaperChanged(object param)
        {
            SelectedWallpaper = param.ToString();
            wallpaperWindow.Close();

            previewWallpaperWindow = new Views.PreviewWallpaperWindow();
            previewWallpaperWindow.Closed += PreviewWallpaperClosed;
            previewWallpaperWindow.DataContext = this;
            previewWallpaperWindow.ShowDialog();
            previewWallpaperWindow.Close();
        }

        private void DeviceWallpaperShow(object param)
        {
            isFromDevice = true;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.png,*.jpg,*jpeg)|*.png;*.jpg;*jpeg";
            if (dialog.ShowDialog() == true)
            {
                SelectedWallpaper = dialog.FileName;
                previewWallpaperWindow = new Views.PreviewWallpaperWindow();
                previewWallpaperWindow.DataContext = this;
                previewWallpaperWindow.ShowDialog();
                previewWallpaperWindow.Close();
            }
        }

        private void PreviewWallpaperShow(object param)
        {
            wallpaperWindow.Close();
            previewWallpaperWindow = new Views.PreviewWallpaperWindow();
            previewWallpaperWindow.DataContext = this;
            previewWallpaperWindow.Closed += PreviewWallpaperClosed;
            previewWallpaperWindow.ShowDialog();
            wallpaperWindow.Close();
        }

        private void PreviewWallpaperClosed(object sender, EventArgs e)
        {
            wallpaperWindow = new Views.DefaultWallpaperWindow();
            wallpaperWindow.DataContext = this;
            wallpaperWindow.ShowDialog();
        }

        private void ConfirmWallpaper(object param)
        {
            if (isFromDevice)
            {
                foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Settings\\Wallpaper"))
                    File.Delete(file);
                string path = AppDomain.CurrentDomain.BaseDirectory + "Settings\\Wallpaper\\" + selectedWallpaper.Substring(selectedWallpaper.LastIndexOf('\\') + 1);
                File.Copy(selectedWallpaper, path);
                settings.SelectedWallpaper = path;
            }
            else
            {
                settings.SelectedWallpaper = selectedWallpaper;
            }
            previewWallpaperWindow.Close();
        }

        private void DefaultRingtonShow(object param)
        {
            Views.RingtonWindow window = new Views.RingtonWindow();
            window.DataContext = this;
            window.ShowDialog();
        }

        private void UserRingtonShow(object param)
        {
            Views.RingtonWindow window = new Views.RingtonWindow();
            window.DataContext = new UserRingtonsViewModel(settings);
            window.ShowDialog();
        }

        private void GroupRingtonShow(object param)
        {
            Views.RingtonWindow window = new Views.RingtonWindow();
            window.DataContext = new GroupRingtonsViewModel(settings);
            window.ShowDialog();
        }        

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
