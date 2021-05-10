using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.ViewModels
{
    public class SettingsViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private Settings settings;
        private Views.DefaultWallpaperWindow wallpaperWindow;
        private Views.PreviewWallpaperWindow previewWallpaperWindow;
        private string selectedWallpaper;
        private MediaPlayer player;
        private bool loadCount1IsChecked;
        private bool loadCount2IsChecked;
        private bool loadCount3IsChecked;
        private bool isFromDevice;


        public SettingsViewModel(ClientUserInfo client)
        {
            Client = client;
            settings = Settings.Instance;
            player = new MediaPlayer();

            MessageLoadCountChangedCommand = new Command(MessageLoadCountChanged);
            DefaultWallpaperShowCommand = new Command(DefaultWallpaperShow);
            DeviceWallpaperShowCommand = new Command(DeviceWallpaperShow);
            DefaultWallpaperChangedCommand = new Command(DefaultWallpaperChanged);
            PreviewWallpaperShowCommand = new Command(PreviewWallpaperShow);
            ConfirmWallpaperCommand = new Command(ConfirmWallpaper);
            DefaultRingtonShowCommand = new Command(DefaultRingtonShow);
            DeviceRingtonShowCommand = new Command(DeviceRingtonShow);
            PlayRingtonCommand = new Command(PlayRington);

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
        public ICommand PlayRingtonCommand { get; }

        public ClientUserInfo Client { get; }
        public Settings Settings { get => settings; set => Set(ref settings, value); }
        public string[] DefautWallpapers { get; }
        public string[] DefaultRingtons { get; }
        public string[] DefaultRingtonNames { get; }
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

        private void DeviceRingtonShow(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio files (*.mp3,*.wav)|*.mp3;*.wav";
            if (dialog.ShowDialog() == true)
            {
                foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Settings\\Rington"))
                    File.Delete(file);

                string fileName = dialog.FileName.Substring(dialog.FileName.LastIndexOf('\\') + 1);
                string path = AppDomain.CurrentDomain.BaseDirectory + "Settings\\Rington\\" + fileName;
                File.Copy(dialog.FileName, path);
                settings.SelectedRington = new Rington(fileName, dialog.FileName);
            }
        }

        private void PlayRington(object param)
        {
            Rington rington = (Rington)param;
            settings.SelectedRington = rington;
            player.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + rington.Path));
            player.Play();
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
