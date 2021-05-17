using Client.ChatService;
using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Client.ViewModels
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged, ChatCallback, IHelperUC
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event UCChangedEventHandler RemoveUC;
        public event UCChangedEventHandler AddUC;

        public delegate void ChatDelegate(Models.Chat chat);

        private ClientUserInfo client;

        private ObservableCollection<Models.Chat> chats;
        private Models.Chat selectedChat;

        private UserControl currentView;

        public MainViewModel(string name, int sqlId)
        {
            ChatClient = new ChatClient(new InstanceContext(this));
            Client = new ClientUserInfo(sqlId, name);
            ChatClient.Connect(sqlId, name);

            LoadedWindowCommand = new Command(LoadedWindow);
            ClosedWindowCommand = new Command(ClosedWindow);
            SelectedChatChangedCommand = new Command(SelectedChatChanged);
            CreateChatCommand = new Command(CreateChat);
            SettingsCommand = new Command(Settings);
            ChangeAvatarCommand = new Command(ChangeAvatar);

            Users = new ObservableCollection<KeyValuePair<int, AvailableUser>>();
            Chats = new ObservableCollection<Models.Chat>();

            Users.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
               delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
               {
                   if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                   {
                       DownloadAvatarAsync(((KeyValuePair<int, AvailableUser>)e.NewItems[0]).Value);
                   }
               });
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedChatChangedCommand { get; }
        public ICommand CreateChatCommand { get; }
        public ICommand SettingsCommand { get; }

        public ICommand ChangeAvatarCommand { get; }

        public ChatClient ChatClient { set; get; } // Сеанс
        public ClientUserInfo Client { get => client; set => Set(ref client, value); } // Вся информация о подключенном юзере

        public ObservableCollection<KeyValuePair<int, AvailableUser>> Users { get; private set; }
        public ObservableCollection<Models.Chat> Chats { get => chats; set => Set(ref chats, value); }
        public Models.Chat SelectedChat { get => selectedChat; set { Set(ref selectedChat, value); if (selectedChat == null) RemoveUC.Invoke(currentView); } }

        public void LoadedWindow(object sender)
        {
            try
            {
                client.DownloadAvatarAsync();
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

        private async void ChangeAvatar(object obj)
        {
            MessageBox.Show("!!!");
            UploadFileInfo uploadFileInfo = null;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == true)
                {

                    MemoryStream memoryStream = ImageCropper.GetCroppedImage(openFileDialog.FileName);
                    memoryStream.Position = 0;

                    uploadFileInfo = new ChatService.UploadFileInfo { FileName = openFileDialog.FileName, FileStream = memoryStream, Responsed_SqlId = client.SqlId };
                    ChatService.AvatarClient avatarClient = new ChatService.AvatarClient();

                    if (uploadFileInfo.FileStream.CanRead)
                        await avatarClient.UserAvatarUploadAsync(uploadFileInfo.FileName, uploadFileInfo.Responsed_SqlId, uploadFileInfo.FileStream);

                    memoryStream.Position = 0;

                    var bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    client.UserImage = bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                if (uploadFileInfo != null)
                {
                    if (uploadFileInfo.FileStream != null)
                        uploadFileInfo.FileStream.Dispose();
                }
            }
        }

        public void SelectedChatChanged(object param)
        {
            if(selectedChat != null)
                RemoveUC.Invoke(currentView);
            Views.ChatUC chatView = new Views.ChatUC();
            ChatViewModel viewModel = new ChatViewModel(this);
            chatView.getControl = new Views.ChatUC.GetControlDelegate(viewModel.SetScrollViewer);
            chatView.DataContext = viewModel;
            currentView = chatView;
            AddUC.Invoke(currentView);
        }

        private void CreateChat(object param)
        {
            RemoveUC.Invoke(currentView);
            UserControl control = new Views.CreateGroupUC();
            CreateChatViewModel viewModel = new CreateChatViewModel(this);
            control.DataContext = viewModel;
            currentView = control;
            AddUC.Invoke(currentView);
        }

        private void Settings(object param)
        {
            foreach (var item in chats)
            {
                item.UserIsWriting = null;
                item.LastMessage = new FileMessage("aaa.docx", new DateTime(1999, 10, 10));
            }
            RemoveUC.Invoke(currentView);
            UserControl control = new Views.SettingsUC();
            control.DataContext = new SettingsViewModel(Client);
            currentView = control;
            AddUC.Invoke(currentView);
        }

        private void RemoveUserFromChatroom(int chatId, int userId)
        {
            var chat = FindChatroom(chatId);
            chat.UserLeft(userId);
        }

        private void UserOnlineState(int userId, bool state)
        {
            foreach (var chat in chats)
            {
                if (chat.SetOnlineState(userId, state))
                {
                    chat.UserIsWriting = "";
                }
            }
        }

        private void UserRemovedFromChat(int chatId)
        {
            ChatGroup chat = FindChatroom(chatId) as ChatGroup;
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

        //private void RemoveChatFromChats(Models.Chat chat)
        //{
        //    Chats.Remove(chat);
        //}

        private void ChatIsWriting(int chatId, int? userId)
        {
            var chat = FindChatroom(chatId);
            chat.MessageIsWriting(userId);
        }

        public void NotifyUserIsOnline(int sqlUserId)
        {
            UserOnlineState(sqlUserId, true);
        }

        public void NotifyUserIsOffline(int sqlUserId)
        {
            UserOnlineState(sqlUserId, false);
        }

        public void NotifyUserIsAddedToChat(int chatId, string chatName, ChatService.UserInChat[] usersInChat)
        {
            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    List<AvailableUser> availableUsers = new List<AvailableUser>();
                    foreach (var item in usersInChat)
                    {
                        AvailableUser user = Users.FirstOrDefault(u => u.Key == item.UserSqlId).Value;
                        if (user == null)
                        {
                            user = new AvailableUser(item.UserSqlId, item.UserName, item.IsOnline);
                            Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                        }
                        availableUsers.Add(user);
                    }

                    Chats.Add(availableUsers.Count > 1 ? new ChatGroup(chatId, chatName, availableUsers) { CanWrite = true }
                                                   : (Models.Chat)new ChatOne(chatId, availableUsers.First()) { CanWrite = true });
                });
            });

            ChatClient.AddedUserToChatIsOnline(this.client.SqlId, chatId);
        }

        public void NotifyUserIsRemovedFromChat(int chatId)
        {
            UserRemovedFromChat(chatId);
        }

        public void UserJoinedToChatroom(int userId)
        {
            //throw new NotImplementedException();
        }

        public void UserLeftChatroom(int chatId, int userId)
        {
            RemoveUserFromChatroom(chatId, userId);
            if (chats.Where(c => c.FindUser(userId) != null).ToList().Count < 2)
            {
                KeyValuePair<int, AvailableUser> availableUser = Users.First(u => u.Key == userId);
                Users.Remove(availableUser);
            }
        }

        public void ReplyMessage(ServiceMessageText message, int chatroomId)
        {
            var chat = FindChatroom(chatroomId);
            if (chat != null)
            {
                chat.Messages.Add(chat.GetMessageType(Client.SqlId, message.UserId, new TextMessage(message.Text, message.DateTime)));
            }
        }

        public void ReplyMessageIsWriting(Nullable<int> userSqlId, int chatSqlId)
        {
            ChatIsWriting(chatSqlId, userSqlId);
        }

        public void NotifyUserSendedFileToChat(ServiceMessageFile serviceMessageFile, int chatroomId)
        {
            var chat = FindChatroom(chatroomId);
            if (chat != null)
                chat.Messages.Add(chat.GetMessageType(Client.SqlId, serviceMessageFile.UserId, new FileMessage(serviceMessageFile.FileName, serviceMessageFile.DateTime, serviceMessageFile.StreamId)));
        }

        public void ClosedWindow(object sender)
        {

        }

        private async void LoadChatroomsAsync()
        {
            Dictionary<Chatroom, UserInChat[]> chatrooms = await ChatClient.FindAllChatroomsForClientAsync(client.SqlId);
            Chats = new ObservableCollection<Models.Chat>
                (await System.Threading.Tasks.Task<List<Models.Chat>>.Run(() =>
            {
                List<Models.Chat> clientChatrooms = new List<Models.Chat>();
                foreach (Chatroom key in chatrooms.Keys)
                {
                    bool canWrite = false;
                    if (chatrooms[key].Length > 1)
                    {
                        List<AvailableUser> availableUsers = new List<AvailableUser>();
                        foreach (UserInChat userInChat in chatrooms[key])
                        {
                            if (userInChat.UserSqlId == client.SqlId)
                            {
                                canWrite = true;
                                continue;
                            }

                            AvailableUser user = Users.FirstOrDefault(u => u.Key == userInChat.UserSqlId).Value;
                            if (user == null)
                            {
                                user = new AvailableUser(userInChat.UserSqlId, userInChat.UserName, userInChat.IsOnline);
                                Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                            }
                            availableUsers.Add(user);
                        }
                        clientChatrooms.Add(availableUsers.Count > 1 ? new ChatGroup(key.ChatSqlId, key.ChatName, availableUsers) { CanWrite = canWrite }
                                                                 : (Models.Chat)new ChatOne(key.ChatSqlId, availableUsers[0]) { CanWrite = canWrite });
                    }
                    else
                    {
                        clientChatrooms.Add(new ChatOne(key.ChatSqlId) { CanWrite = canWrite });
                    }
                }
                return clientChatrooms;
            }));
        }

        public async void DownloadAvatarAsync(AvailableUser user)
        {
            ChatService.DownloadRequest request = new ChatService.DownloadRequest(user.SqlId);
            var avatarClient = new ChatService.AvatarClient();
            Stream stream = null;
            MemoryStream memoryStream = null;
            long lenght = 0;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    avatarClient.UserAvatarDownload(user.SqlId, out lenght, out stream);
                    if (lenght <= 0)
                        return;
                    memoryStream = FileHelper.ReadFileByPart(stream);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();

                        user.Image = bitmapImage;
                    });

                });

            }
            catch (System.ServiceModel.FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                throw ex;
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Close();
                    memoryStream.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public void NotifyUserChangedAvatar(int userId)
        {
            AvailableUser user = Users.FirstOrDefault(u => u.Key == userId).Value;
            if (user != null)
                DownloadAvatarAsync(user);
        }

        public void NotifyСhatroomAvatarIsChanged(int chatId)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

        //private void Demo()
        //{
        //    ObservableCollection<SourceMessage> Messages = new ObservableCollection<SourceMessage>();

        //    Messages.Add(new SystemMessage(new TextMessage("User type messages")));
        //    Messages.Add(new UserMessage(new TextMessage("hello group asd asfas fasf asfas fasf asfpKJAOPSKfa")));
        //    Messages.Add(new UserMessage(new FileMessage("files/hall.png") { IsLoaded = false }));
        //    Messages.Add(new UserMessage(new MediaMessage("files/control.mp3")));
        //    Messages.Add(new UserMessage(new FileMessage("files/1.txt") { IsLoaded = true }));
        //    Messages.Add(new UserMessage(new FileMessage("files/1.txt") { IsLoaded = false }));

        //    Messages.Add(new SystemMessage(new TextMessage("Friend type messages")));
        //    Messages.Add(new SourceMessage(new TextMessage("hello group asd asfas fasf asfas fasf asfpKJAOPSKfa")));
        //    Messages.Add(new SourceMessage(new FileMessage("files/hall.png") { IsLoaded = false }));
        //    Messages.Add(new SourceMessage(new MediaMessage("files/control.mp3")));
        //    Messages.Add(new SourceMessage(new FileMessage("files/1.txt") { IsLoaded = true }));
        //    Messages.Add(new SourceMessage(new FileMessage("files/1.txt") { IsLoaded = false }));

        //    Messages.Add(new SystemMessage(new TextMessage("Group type messages")));
        //    Messages.Add(new GroupMessage(new TextMessage("hello group asd asfas fasf asfas fasf asfpKJAOPSKfa"), new AvailableUser() { Name = "eldar" }));
        //    Messages.Add(new GroupMessage(new FileMessage("files/hall.png") { IsLoaded = false }, new AvailableUser() { Name = "Tamerlan" }));
        //    Messages.Add(new GroupMessage(new MediaMessage("files/control.mp3"), new AvailableUser() { Name = "eldar" }));
        //    Messages.Add(new GroupMessage(new FileMessage("files/1.txt") { IsLoaded = true }, new AvailableUser() { Name = "Eldar" }));
        //    Messages.Add(new GroupMessage(new FileMessage("files/1.txt") { IsLoaded = false }, new AvailableUser() { Name = "Tamerlan" }));

        //    //Chats.Add(new ChatOne() { User = new AvailableUser() { Name = "eldar" }, Messages = Messages });
        //    //Chats.Add(new ChatGroup() { GroupName = "group1", Messages = Messages });

        //    BitmapImage image = new BitmapImage(new Uri("files/avatar1.png", UriKind.Relative));
        //    Chats.Add(new ChatOne() { Avatar = image, Messages = Messages });

        //    image = new BitmapImage(new Uri("files/avatar2.png", UriKind.Relative));
        //    Chats.Add(new ChatGroup() { Avatar = image, Messages = Messages });

        //    image = new BitmapImage(new Uri("files/avatar3.png", UriKind.Relative));
        //    Chats.Add(new ChatOne() { Avatar = image, Messages = Messages });

        //    //Messages.Add(new GroupMessage(new TextMessage("Hello !!!!"), "User2"));
        //    //Messages.Add(new GroupMessage(new FileMessage("files/hall.png"), "User3"));
        //    //Messages.Add(new GroupMessage(new FileMessage("files/2.pptx"), "Eldar"));
        //    //Messages.Add(new GroupMessage(new MediaMessage("files/control.mp3"), "Emil"));
        //}
    }
}
