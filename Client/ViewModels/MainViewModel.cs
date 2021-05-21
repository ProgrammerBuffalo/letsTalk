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
            CancelImageCommand = new Command(CancelImage);

            currentView = new Views.HelpUC();

            Users = new ObservableCollection<KeyValuePair<int, AvailableUser>>();
            Chats = new ObservableCollection<Models.Chat>();

            Users.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
               delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
               {
                   if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                   {
                       DownloadUserAvatarAsync(((KeyValuePair<int, AvailableUser>)e.NewItems[0]).Value);
                   }
               });

            Chats.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
             delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
             {
                 if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                 {
                     Models.Chat chat = Chats[Chats.IndexOf(e.NewItems[0] as Models.Chat)];
                     if (e.NewItems[0] is Models.ChatGroup)
                         DownloadChatAvatarAsync(e.NewItems[0] as Models.ChatGroup);
                     chat.LastMessage = new TextMessage("You are added", DateTime.Now);
                 }
              
             });
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedChatChangedCommand { get; }
        public ICommand CreateChatCommand { get; }
        public ICommand SettingsCommand { get; }

        public ICommand ChangeAvatarCommand { get; }
        public ICommand CancelImageCommand { get; }

        public ChatClient ChatClient { set; get; } // Сеанс
        public ClientUserInfo Client { get => client; set => Set(ref client, value); } // Вся информация о подключенном юзере

        public ObservableCollection<KeyValuePair<int, AvailableUser>> Users { get; private set; }
        public ObservableCollection<Models.Chat> Chats { get => chats; set => Set(ref chats, value); }
        public Models.Chat SelectedChat { get => selectedChat; set { if (selectedChat == null) RemoveUC.Invoke(currentView); Set(ref selectedChat, value); } }

        public async void LoadedWindow(object sender)
        {
            try
            {
                client.DownloadAvatarAsync();
                AddUC.Invoke(currentView);
                await LoadChatroomsAsync();
                foreach (var item in chats)
                {
                    if (item is ChatGroup)
                        DownloadChatAvatarAsync(item as ChatGroup);

                    item.LastMessage = await Utility.MessageLoader.LoadMessage(item, client.SqlId, 1, 1);
                    if(item.LastMessage is TextMessage)
                    {
                        TextMessage textMessage = item.LastMessage as TextMessage;
                        DateTime dateTime = DateTime.MinValue;
                        if(DateTime.TryParse(textMessage.Text, out dateTime))
                        {
                            dateTime = new ChatService.UnitClient().FindUserJoin(this.client.SqlId, item.SqlId);
                            item.LastMessage = new TextMessage("You are added", dateTime);
                        }
                    }
                }

                Chats.Sort((a, b) => { return b.LastMessage.Date.CompareTo(a.LastMessage.Date); });

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

        public void CancelImage(object param)
        {
            client.UserImage = new BitmapImage(new Uri("Resources/user.png", UriKind.Relative));
            new ChatService.UnitClient().UserAvatarDelete(client.SqlId);
        }

        private async void ChangeAvatar(object obj)
        {
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
            if (selectedChat != null)
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
            RemoveUC.Invoke(currentView);
            UserControl control = new Views.SettingsUC();
            control.DataContext = new SettingsViewModel(Client);
            currentView = control;
            AddUC.Invoke(currentView);
        }

        private void RemoveUserFromChatroom(int chatId, int userId)
        {
            var chat = FindChatroom(chatId);
            if (chat != null)
                chat.UserLeft(userId);
        }

        private void UserOnlineState(int userId, bool state)
        {
            foreach (var chat in chats)
            {
                if (chat.SetOnlineState(userId, state))
                {
                    chat.UserIsWriting = null;
                }
            }
        }

        private void UserRemovedFromChat(int chatId)
        {
            ChatGroup chat = FindChatroom(chatId) as ChatGroup;
            if (chat != null)
            {
                List<int> usersOfChat = chat.Users.Select(u => u.SqlId).ToList();
                foreach (var item in usersOfChat)
                {
                    int count = Chats.Select(c => c.FindUser(item)).ToList().Count;
                    if (count < 2)
                        Users.Remove(Users.First(u => u.Key == item));
                }
                chat.LastMessage = new TextMessage("You are removed", DateTime.Now);
                Chats.Move(Chats.IndexOf(chat), 0);
                chat.CanWrite = false;                
            }

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

        public void NotifyUserIsAddedToChat(int chatId, string chatName, ChatService.UserInChat[] usersInChat, bool isGroup)
        {
            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    List<AvailableUser> availableUsers = new List<AvailableUser>();
                    foreach (var item in usersInChat)
                    {
                        if (item.UserSqlId == client.SqlId)
                            continue;

                        AvailableUser user = Users.FirstOrDefault(u => u.Key == item.UserSqlId).Value;
                        if (user == null)
                        {
                            user = new AvailableUser(item.UserSqlId, item.UserName, item.IsOnline);
                            Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                        }
                        availableUsers.Add(user);
                    }

                    this.Chats.Add(isGroup ? new ChatGroup(chatId, chatName, availableUsers) { CanWrite = true } :
                                        (Models.Chat)new ChatOne(chatId, availableUsers.First()) { CanWrite = true });
                    Chats.Move(Chats.IndexOf(Chats.Last()), 0);

                });

                ChatClient.AddedUserToChatIsOnline(this.client.SqlId, chatId);
            });
        }

        public void NotifyUserIsRemovedFromChat(int chatId)
        {
            UserRemovedFromChat(chatId);
        }

        public void UserJoinedToChatroom(int chatId, int userId, string userName)
        {
            var chat = Chats.FirstOrDefault(c => c.SqlId == chatId);
            if (chat != null)
            {
                AvailableUser user = Users.FirstOrDefault(u => u.Key == userId).Value;
                if (user == null)
                {
                    user = new AvailableUser(userId, userName);
                    Users.Add(new KeyValuePair<int, AvailableUser>(userId, user));
                }
                (chat as ChatGroup).AddMember(user);
                chat.LastMessage = SystemMessage.UserAdded(DateTime.Now, user.Name).Message;
            }
        }

        public void UserLeftChatroom(int chatId, int userId)
        {
            var chat = chats.FirstOrDefault(c => c.SqlId == chatId);
            if (chat != null)
                chat.LastMessage = SystemMessage.UserLeftChat(DateTime.Now, new ChatService.UnitClient().FindUserName(userId)).Message;
            RemoveUserFromChatroom(chatId, userId);

            if (chats.Where(c => c.FindUser(userId) != null).ToList().Count < 1)
            {
                Nullable<KeyValuePair<int, AvailableUser>> availableUser = Users.FirstOrDefault(u => u.Key == userId);
                if (availableUser != null)
                {
                    availableUser.Value.Value.Image = null;
                    availableUser.Value.Value.IsOnline = false;
                    Users.Remove(availableUser.Value);
                }
            }

        }

        public void ReplyMessage(ServiceMessageText message, int chatroomId)
        {
            var chat = FindChatroom(chatroomId);
            chat.LastMessage = new TextMessage(message.Text, message.DateTime);

            if(Chats.IndexOf(chat) != 0)
                Chats.Move(Chats.IndexOf(chat), 0);

            if (SelectedChat == null)
                return;

            if (SelectedChat.Equals(chat))
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
            chat.LastMessage = new FileMessage(serviceMessageFile.FileName, serviceMessageFile.DateTime);

            if (Chats.IndexOf(chat) != 0)
                Chats.Move(Chats.IndexOf(chat), 0);

            if (SelectedChat == null)
                return;

            if (SelectedChat.Equals(chat))
                chat.Messages.Add(chat.GetMessageType(Client.SqlId, serviceMessageFile.UserId, new FileMessage(serviceMessageFile.FileName, serviceMessageFile.DateTime, serviceMessageFile.StreamId)));
        }

        public void ClosedWindow(object sender)
        {

        }

        private async Task LoadChatroomsAsync()
        {
            Dictionary<Chatroom, UserInChat[]> chatrooms = await ChatClient.FindAllChatroomsForClientAsync(client.SqlId);
            await System.Threading.Tasks.Task.Run(() =>
            {
                List<Models.Chat> clientChatrooms = new List<Models.Chat>();
                foreach (Chatroom key in chatrooms.Keys)
                {
                    bool canWrite = false;

                    UserInChat requestedUser = chatrooms[key].FirstOrDefault(u => u.UserSqlId == client.SqlId);
                    if (requestedUser != null)
                    {
                        if (requestedUser.IsLeft)
                            continue;
                    }

                    if (key.IsGroup)
                    {

                        List<AvailableUser> availableUsers = new List<AvailableUser>();
                        foreach (UserInChat userInChat in chatrooms[key])
                        {

                            if (userInChat.UserSqlId == client.SqlId)
                            {
                                if (requestedUser.LeaveDate != DateTime.MinValue)
                                {
                                    canWrite = false;
                                }
                                else
                                {
                                    canWrite = true;
                                }
                                continue;
                            }

                            if (userInChat.LeaveDate != DateTime.MinValue)
                                continue;

                            AvailableUser user = Users.FirstOrDefault(u => u.Key == userInChat.UserSqlId).Value;
                            if (user == null)
                            {
                                user = new AvailableUser(userInChat.UserSqlId, userInChat.UserName, userInChat.IsOnline);
                                Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                            }

                            availableUsers.Add(user);
                        }
                        App.Current.Dispatcher.Invoke(() => Chats.Add(new ChatGroup(key.ChatSqlId, key.ChatName, availableUsers) { CanWrite = canWrite }));
                    }
                    else
                    {
                        UserInChat friend = chatrooms[key].FirstOrDefault(usr => usr.UserSqlId != client.SqlId);
                        AvailableUser user = Users.FirstOrDefault(u => u.Key == friend.UserSqlId).Value;
                        if (!friend.IsLeft)
                        {
                            if (user == null)
                            {
                                user = new AvailableUser(friend.UserSqlId, friend.UserName, friend.IsOnline);
                                Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                            }
                        }
                        else
                        {
                            user = new AvailableUser(friend.UserSqlId, friend.UserName, friend.IsOnline);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                user.Image = new BitmapImage(new Uri("Resources/user.png", UriKind.Relative));
                            });
                        }
                        App.Current.Dispatcher.Invoke(() => Chats.Add(new ChatOne(key.ChatSqlId, user) { CanWrite = !friend.IsLeft }));
                    }
                }
            });

        }

        public async void DownloadUserAvatarAsync(AvailableUser user)
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
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            user.Image = new BitmapImage(new Uri("Resources/user.png", UriKind.Relative));
                        });
                        return;
                    }
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

        public async void DownloadChatAvatarAsync(Models.ChatGroup chat)
        {
            ChatService.DownloadRequest request = new ChatService.DownloadRequest(chat.SqlId);
            var avatarClient = new ChatService.AvatarClient();
            Stream stream = null;
            MemoryStream memoryStream = null;
            long lenght = 0;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    avatarClient.ChatAvatarDownload(chat.SqlId, out lenght, out stream);
                    if (lenght <= 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            chat.Avatar = new BitmapImage(new Uri("Resources/group.png", UriKind.Relative));
                        });
                        return;
                    }
                    memoryStream = FileHelper.ReadFileByPart(stream);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();

                        chat.Avatar = bitmapImage;
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
                DownloadUserAvatarAsync(user);
        }

        public void NotifyСhatroomAvatarIsChanged(int chatId)
        {
            var chat = FindChatroom(chatId);
            if (chat != null)
                DownloadChatAvatarAsync(chat as ChatGroup);
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
