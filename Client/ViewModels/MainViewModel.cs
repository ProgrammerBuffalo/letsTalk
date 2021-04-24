using Client.ChatService;
using Client.Models;
using Client.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged, ChatCallback
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public delegate void ChatDelegate(Models.Chat chat);

        private ClientUserInfo clientUserInfo;
        private ChatClient ChatClient { set; get; } // Сеанс

        private ObservableCollection<Models.Chat> chats;
        private Models.Chat selectedChat;

        private ContentControl currentView;

        public MainViewModel()
        {
            LoadedWindowCommand = new Command(LoadedWindow);
            ClosedWindowCommand = new Command(ClosedWindow);
            SelectedChatChangedCommand = new Command(SelectedChatChanged);
            AddUserCommand = new Command(AddUser);
            CreateGroupCommand = new Command(CreateGroup);
            SettingsCommand = new Command(Settings);

            Chats = new ObservableCollection<Models.Chat>();

            //SelectedHambugerOptionItemCommand = new Command(SelectedHambugerOptionItem);
        }

        public MainViewModel(string name, int sqlId) : this()
        {
            ChatClient = new ChatClient(new InstanceContext(this));
            ClientUserInfo = new ClientUserInfo(sqlId, name);

            ChatClient.Connect(sqlId, name);
            //Demo();
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
        public Models.Chat SelectedChat { get => selectedChat; set => Set(ref selectedChat, value); }

        public ContentControl CurrentView { get => currentView; set => Set(ref currentView, value); }

        // После того как окно полностью прогрузилось, у нас происходит вызов загрузки аватарки с сервера к пользователю
        public void LoadedWindow(object sender)
        {
            try
            {
                clientUserInfo.DownloadAvatarAsync();
                LoadChatroomsAsync();
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

        //public void SelectedHambugerOptionItem(object sender)
        //{
        //    HamburgerMenuIconItem menuIconItem = sender as HamburgerMenuIconItem;
        //    Type userControl = Type.GetType("Client.Views." + menuIconItem.Tag);

        //    CurrentView = Activator.CreateInstance(userControl, ChatClient) as ContentControl;
        //}

        public void SelectedChatChanged(object param)
        {
            if (selectedChat != null)
            {
                //Models.Chat chat = (Models.Chat)param;
                Views.UCChat chatView = new Views.UCChat();
                ChatViewModel viewModel = new ChatViewModel();
                viewModel.RemoveChat += RemoveChatFromChats;
                chatView.DataContext = new ChatViewModel(selectedChat, clientUserInfo);
                CurrentView = chatView;
            }
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
            viewModel.AddChat += AddChatToChats;
            control.DataContext = viewModel;
            CurrentView = control;
        }

        private void Settings(object param)
        {
            UserControl control = new Views.UCSettings();
            control.DataContext = new AddUserViewModel();
            CurrentView = control;
        }

        private void UserLeavedChatroom(int chatId, int userId)
        {
            var chat = FindChatroom(chatId);
            chat.UserLeavedChatroom(userId);
        }

        private void UserOnlineState(int userId, bool state)
        {
            foreach (var chat in chats)
            {
                chat.SetOnlineState(userId, state);
                break;
            }
        }

        private void UserAddedToChat(int chatId, int userId)
        {

        }

        private void UserRemovedFromChat(int chatId, int userId)
        {
            var chat = FindChatroom(chatId);
            (chat as ChatGroup).RemoveMember(userId);
        }

        private Models.Chat FindChatroom(int chatId)
        {
            foreach (var chat in chats)
                if (chat.SqlId == chatId)
                    return chat;
            return null;
        }

        private void AddChatToChats(Models.Chat chat)
        {
            Chats.Add(chat);
        }

        private void RemoveChatFromChats(Models.Chat chat)
        {
            Chats.Remove(chat);
        }

        private void ChatIsWriting(int chatId, int userId, bool state)
        {
            var chat = FindChatroom(chatId);
            chat.MessageIsWriting(userId, state);
        }

        public void NotifyUserIsOnline(int sqlUserId)
        {
            //UserOnlineState(userId, true);
            throw new NotImplementedException();
        }

        public void NotifyUserIsOffline(int sqlUserId)
        {
            //UserOnlineState(userId, false);
            throw new NotImplementedException();
        }

        public void NotifyUserIsAddedToChat(int chatId, int[] usersInChat)
        {
            throw new NotImplementedException();
        }

        public void NotifyUserIsRemovedFromChat(int chatId)
        {
            //UserRemovedFromChat(chatId, userId);
            throw new NotImplementedException();
        }

        public void UserJoinedToChatroom(int userId)
        {
            throw new NotImplementedException();
        }

        public void UserLeftChatroom(int userId)
        {
            //UserLeavedChatroom(chatId, userId);
            throw new NotImplementedException();
        }

        public void ReplyMessage(ServiceMessageText message, int chatroomId)
        {
            throw new NotImplementedException();
        }

        public void ReplyMessageIsWriting(int sqlId)
        {
            //ChatIsWriting(chatId, userId, state);
            throw new NotImplementedException();
        }

        public void NotifyUserFileSendedToChat(ServiceMessageFile serviceMessageFile, int chatroomId)
        {
            throw new NotImplementedException();
        }

        public void ClosedWindow(object sender)
        {
            ChatClient.Disconnect();
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

        private async void LoadChatroomsAsync()
        {
            Dictionary<Chatroom, UserInChat[]> chatrooms = await ChatClient.FindAllChatroomsForClientAsync(clientUserInfo.SqlId);
            Chats = new ObservableCollection<Models.Chat>
                (await System.Threading.Tasks.Task<List<Models.Chat>>.Run(() =>
            {
                List<Models.Chat> clientChatrooms = new List<Models.Chat>();
                foreach (Chatroom key in chatrooms.Keys)
                {
                    List<AvailableUser> availableUsers = new List<AvailableUser>();
                    foreach (UserInChat userInChat in chatrooms[key])
                    {
                        availableUsers.Add(new AvailableUser { SqlId = userInChat.UserSqlId, Name = userInChat.UserName });
                    }

                    clientChatrooms.Add(availableUsers.Count > 1 ? (Models.Chat)new ChatGroup(key.ChatSqlId, key.ChatName, availableUsers)
                                                                 : new ChatOne(key.ChatSqlId, availableUsers.First()));
                }
                return clientChatrooms;
            }));

            foreach (Models.Chat chat in chats)
            {
                chat.DownloadAvatarAsync();
            }

        }

        private void Demo()
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
            Messages.Add(SystemMessage.UserLeavedChat("someone"));

            Chats.Add(new ChatOne() { User = new AvailableUser() { Name = "eldar" }, Messages = Messages });
            Chats.Add(new ChatGroup() { GroupName = "group1", Messages = Messages });
            //Messages.Add(new GroupMessage(new TextMessage("Hello !!!!"), "User2"));
            //Messages.Add(new GroupMessage(new FileMessage("files/hall.png"), "User3"));
            //Messages.Add(new GroupMessage(new FileMessage("files/2.pptx"), "Eldar"));
            //Messages.Add(new GroupMessage(new MediaMessage("files/control.mp3"), "Emil"));
        }
    }
}
