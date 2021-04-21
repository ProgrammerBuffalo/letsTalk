using Client.ChatService;
using Client.Models;
using Client.Utility;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged, ChatCallback
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ClientUserInfo clientUserInfo;

        private ChatClient ChatClient { set; get; } // Сеанс

        private ObservableCollection<Models.Chat> chats;

        private ContentControl currentView;

        public Dictionary<int, int[]> ChatroomsWithUsers { get; set; }

        public MainViewModel()
        {
            LoadedWindowCommand = new Command(LoadedWindow);
            ClosedWindowCommand = new Command(ClosedWindow);
            SelectedHambugerOptionItemCommand = new Command(SelectedHambugerOptionItem);
            SelectedChatChangedCommand = new Command(SelectedChatChanged);
            AddUserCommand = new Command(AddUser);
            CreateGroupCommand = new Command(CreateGroup);
            SettingsCommand = new Command(Settings);

            Chats = new ObservableCollection<Models.Chat>();
        }

        public MainViewModel(string name, int sqlId) : this()
        {
            ChatClient = new ChatClient(new InstanceContext(this));
            ChatroomsWithUsers = ChatClient.Connect(sqlId, name);

            ClientUserInfo = new ClientUserInfo(sqlId, name);

            UnitClient unitClient = new UnitClient();
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedHambugerOptionItemCommand { get; }
        public ICommand SelectedChatChangedCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand CreateGroupCommand { get; }
        public ICommand SettingsCommand { get; }

        public ClientUserInfo ClientUserInfo { get => clientUserInfo; set => Set(ref clientUserInfo, value); } // Вся информация о подключенном юзере

        public ObservableCollection<Models.Chat> Chats { get => chats; set => Set(ref chats, value); }

        public ContentControl CurrentView { get => currentView; set => Set(ref currentView, value); }

        public void SelectedHambugerOptionItem(object sender)
        {
            //HamburgerMenuIconItem menuIconItem = sender as HamburgerMenuIconItem;
            //Type userControl = Type.GetType("Client.Views." + menuIconItem.Tag);

            //CurrentView = Activator.CreateInstance(userControl, ChatClient) as ContentControl;
        }

        public void SelectedChatChanged(object param)
        {
            Models.Chat chat = (Models.Chat)param;
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
            catch (FaultException<ConnectionExceptionFault> ex)
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
            ChatClient.Disconnect();
        }

        private void AddUser(object param)
        {
            UserControl control = new Views.AddUserUC();
            AddUserViewModel viewModel = new AddUserViewModel(ChatClient);
            viewModel.AddChat += AddChatToChats;
            control.DataContext = viewModel;
            CurrentView = control;
        }

        private void CreateGroup(object param)
        {
            UserControl control = new Views.CreateGroupUC();
            CreateGroupViewModel viewModel = new CreateGroupViewModel(ChatClient);
            viewModel.AddChat += AddGroupToChats;
            control.DataContext = viewModel;
            CurrentView = control;
        }

        private void Settings(object param)
        {
            UserControl control = new Views.UCSettings();
            control.DataContext = new AddUserViewModel();
            CurrentView = control;
        }

        private void AddChatToChats(Models.Chat chat)
        {
            Chats.Add(chat);
            Demo(chat);
        }

        private void AddGroupToChats(Models.Chat chat)
        {
            Chats.Add(chat);
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

        public void NotifyUserIsOnline(int sqlUserId)
        {
            throw new NotImplementedException();
        }

        public void NotifyUserIsOffline(int sqlUserId)
        {
            throw new NotImplementedException();
        }

        public void NotifyUserIsAddedToChat(int chatId, int[] usersInChat)
        {
            throw new NotImplementedException();
        }

        public void NotifyUserIsRemovedFromChat(int chatId)
        {
            throw new NotImplementedException();
        }

        public void UserJoinedToChatroom(int userId)
        {
            throw new NotImplementedException();
        }

        public void UserLeftChatroom(int userId)
        {
            throw new NotImplementedException();
        }

        public void ReplyMessage(ServiceMessageText message, int chatroomId)
        {
            throw new NotImplementedException();
        }

        public void ReplyMessageIsWriting(int sqlId)
        {
            throw new NotImplementedException();
        }

        public void NotifyUserFileSendedToChat(ServiceMessageFile serviceMessageFile, int chatroomId)
        {
            throw new NotImplementedException();
        }


        private void Demo(Models.Chat chat)
        {
            //Chats = new ObservableCollection<Models.Chat>();
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

            chat.Messages = Messages;

            //Chats.Add(new ChatOne(Messages, new ava("Eldar", "Height Logic!!!", "files/user.png", Activity.Offline), 0));

            //ObservableCollection<SourceMessage> Messages1 = new ObservableCollection<SourceMessage>();
            //Messages1.Add(new SourceMessage(new TextMessage("Hello")));
            //Messages1.Add(new UserMessage(new TextMessage("Bye")));

            //Chats.Add(new ChatOne(Messages1, new ClientUserInfo("Kesha", "Fly Forever", "files/pexels-caio-56733.jpg", Activity.Online), 124));
        }
    }
}
