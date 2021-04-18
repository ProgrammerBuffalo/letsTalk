using Client.Models;
using Client.Utility;
using MahApps.Metro.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ClientUserInfo clientUserInfo;

        private ObservableCollection<Chat> chats;

        private ContentControl currentView;

        public MainViewModel()
        {
            LoadedWindowCommand = new Command(LoadedWindow);
            ClosedWindowCommand = new Command(ClosedWindow);
            SelectedHambugerOptionItemCommand = new Command(SelectedHambugerOptionItem);
            SelectedChatChangedCommand = new Command(SelectedChatChanged);
        }

        public MainViewModel(string name, int sqlId) : this()
        {
            ChatService.ChatClient chatClient = new ChatService.ChatClient();
            Guid unique_id = chatClient.Connect(sqlId);

            ClientUserInfo = new ClientUserInfo(unique_id, sqlId, chatClient, name);

            Demo();
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedHambugerOptionItemCommand { get; }
        public ICommand SelectedChatChangedCommand { get; }

        public ClientUserInfo ClientUserInfo { get => clientUserInfo; set => Set(ref clientUserInfo, value); } // Вся информация о подключенном юзере

        public ObservableCollection<Chat> Chats { get => chats; set => Set(ref chats, value); }

        public ContentControl CurrentView { get => currentView; set => Set(ref currentView, value); }

        public void SelectedHambugerOptionItem(object sender)
        {
            HamburgerMenuIconItem menuIconItem = sender as HamburgerMenuIconItem;
            Type userControl = Type.GetType("Client.Views." + menuIconItem.Tag);

            CurrentView = Activator.CreateInstance(userControl) as ContentControl;

            //что это?
            ContentControl contentControl = new ContentControl();
        }

        public void SelectedChatChanged(object param)
        {
            Chat chat = (Chat)param;
            Views.UCChat chatView = new Views.UCChat();
            chatView.DataContext = new ChatViewModel(chat, clientUserInfo);
            CurrentView = chatView;
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

        public void ClosedWindow(object sender)
        {
            ClientUserInfo.ChatClient.Disconnect(ClientUserInfo.ConnectionId);
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

        private void Demo()
        {
            Chats = new ObservableCollection<Chat>();

            ObservableCollection<SourceMessage> Messages = new ObservableCollection<SourceMessage>();
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
            Messages.Add(SystemMessage.UserExitedChat("kesha"));
            Messages.Add(new GroupMessage(new TextMessage("Eldar gde Kahut!!!"), "Dmitriy"));
            Messages.Add(new GroupMessage(new FileMessage("files/hall.png"), "Dmitriy"));
            Messages.Add(new GroupMessage(new FileMessage("files/2.pptx"), "Eldar"));
            Messages.Add(new GroupMessage(new MediaMessage("files/control.mp3"), "Emil"));

            Chats.Add(new ChatOne(Messages, new ClientUserInfo("Eldar", "Height Logic!!!", "files/user.png", Activity.Offline), 0));

            ObservableCollection<SourceMessage> Messages1 = new ObservableCollection<SourceMessage>();
            Messages1.Add(new SourceMessage(new TextMessage("Hello")));
            Messages1.Add(new UserMessage(new TextMessage("Bye")));

            Chats.Add(new ChatOne(Messages1, new ClientUserInfo("Kesha", "Fly Forever", "files/pexels-caio-56733.jpg", Activity.Online), 124));
        }
    }
}
