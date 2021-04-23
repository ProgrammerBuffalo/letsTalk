using Client.Models;
using Client.Utility;
using MahApps.Metro.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<SourceMessage> messages;

        private Message selected_message;

        private MediaMessage curMediaMessage;

        private MediaPlayer player;

        private Timer timer;

        public Window1()
        {
            InitializeComponent();
            DataContext = this;
            MediaPlayCommand = new Command(MediaPlay);
            MediaPosChangedCommand = new Command(MediaPosChanged);

            Messages = new ObservableCollection<SourceMessage>();
            Messages.Add(new SourceMessage(new TextMessage("files/hall.png")));
            Messages.Add(new UserMessage(new MediaMessage("files/Animals.mp3")));
            Messages.Add(new UserMessage(new MediaMessage("files/control.mp3")));
            Messages.Add(new SourceMessage(new TextMessage("asda sdasd asda fas fasd asd asd asda sasd asd")));
            Messages.Add(new UserMessage(new TextMessage("das dasd asda sd asd asda sd")));
            Messages.Add(new SourceMessage(new TextMessage("das dasd asda sd asd asda sd asd asdasd asd asd as das asd as dadsasdfas f asfasf asdf afafasf aas fa saaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa  fas fasf asf       asfasfasfasfaf")));
            Messages.Add(new UserMessage(new FileMessage("files/hall.png")));
            Messages.Add(new UserMessage(new TextMessage("dasdas das dasda sasif jasjf ajf ouiajdijfis jfiasfjos iajsoif jasf")));
            Messages.Add(new UserMessage(new FileMessage("files/1.txt")));
            Messages.Add(new SourceMessage(new FileMessage("files/2.docx")));
            Messages.Add(new UserMessage(new FileMessage("files/3.pptx")));
            Messages.Add(SystemMessage.UserLeavedChat("kesha"));
            //Messages.Add(new GroupMessage(new TextMessage("Eldar gde Kahut!!!"), "Dmitriy"));
            //Messages.Add(new GroupMessage(new FileMessage("files/hall.png"), "Dmitriy"));
            //Messages.Add(new GroupMessage(new FileMessage("files/2.pptx"), "Eldar"));
            //Messages.Add(new GroupMessage(new MediaMessage("files/control.mp3"), "Emil"));
            player = new MediaPlayer();
            player.MediaEnded += MediaEnded;

            timer = new Timer();
            timer.Elapsed += MediaPosTimer;
            timer.Interval = 500;
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            player.Close();
            timer.Stop();
            curMediaMessage.CurrentLength = 0;
            curMediaMessage.IsPlaying = false;
        }

        private void MediaPosTimer(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                curMediaMessage.CurrentLength = player.Position.Ticks;
            });
        }

        public ICommand MediaPlayCommand { get; }
        public ICommand MediaPosChangedCommand { get; }
        public ICommand WindowUnLoadCommand { get; }

        public ObservableCollection<SourceMessage> Messages { get => messages; set => Set(ref messages, value); }
        public Message SelectedMessage { get => selected_message; set => Set(ref selected_message, value); }

        private void MediaPlay(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (player.Source == null)
            {
                curMediaMessage = message;
                player.Open(new Uri(message.Path, UriKind.Absolute));
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
                player.Play();
                timer.Start();
                curMediaMessage.IsPlaying = true;
            }
            else if (message != curMediaMessage)
            {
                curMediaMessage.IsPlaying = false;
                curMediaMessage.CurrentLength = 0;
                player.Close();
                player.Open(new Uri(message.Path, UriKind.Absolute));
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
                player.Play();
                timer.Start();
                curMediaMessage = message;
                curMediaMessage.IsPlaying = true;
            }
            else if (curMediaMessage.IsPlaying)
            {
                player.Pause();
                timer.Stop();
                curMediaMessage.IsPlaying = false;
            }
            else
            {
                player.Play();
                timer.Start();
                curMediaMessage.IsPlaying = true;
            }
        }

        private void MediaPosChanged(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (message == curMediaMessage)
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
        }

        // тут надо при удаление user control вызывать метод
        private void WindowUnLoad(object param)
        {
            if (player != null)
                player.Close();
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
