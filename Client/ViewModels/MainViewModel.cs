using Client.ChatService;
using Client.Models;
using Client.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Notifications.Wpf;

namespace Client.ViewModels
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged, ChatCallback, IHelperUC
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public event UCChangedEventHandler RemoveUC;
        public event UCChangedEventHandler AddUC;

        public delegate void ChatDelegate(Models.Chat chat);

        private ClientUserInfo client;
        private Settings settings;

        private ObservableCollection<Models.Chat> chats;
        private Models.Chat selectedChat;

        private UserControl currentView;
        private System.Windows.Forms.NotifyIcon icon;

        private NotificationManager notifyManager;
        private Views.MainWindow mainWindow;
        private MahApps.Metro.Controls.HamburgerMenuIconItem selectedOptionsItem;

        public MainViewModel(string name, int sqlId)
        {
            mainWindow = (Views.MainWindow)App.Current.MainWindow;
            mainWindow.Closing += WindowClosing;

            ChatClient = new ChatClient(new InstanceContext(this));
            Client = new ClientUserInfo(sqlId, name);
            ChatClient.Connect(sqlId, name);
            settings = Settings.Instance;
            settings.LoadSettings(sqlId);

            LoadedWindowCommand = new Command(LoadedWindow);
            SelectedChatChangedCommand = new Command(SelectedChatChanged);
            CreateChatCommand = new Command(CreateChat);
            SettingsShowCommand = new Command(SettingsShow);
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
                 }

             });

            notifyManager = new NotificationManager();

            icon = new System.Windows.Forms.NotifyIcon();
            icon.Visible = true;
            icon.Text = "lets Talk";
            icon.Icon = new System.Drawing.Icon(AppDomain.CurrentDomain.BaseDirectory + "Resources/logo.ico");
            icon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            icon.MouseClick += ActivateWindow; ;
            System.Windows.Forms.ToolStripButton toolStrip = new System.Windows.Forms.ToolStripButton();
            toolStrip.Text = "Exit";
            toolStrip.Click += ExitFromTray; ;
            icon.ContextMenuStrip.Items.Add(toolStrip);
        }

        public ICommand LoadedWindowCommand { get; }
        public ICommand ClosedWindowCommand { get; }
        public ICommand SelectedChatChangedCommand { get; }
        public ICommand CreateChatCommand { get; }
        public ICommand SettingsShowCommand { get; }

        public ICommand ChangeAvatarCommand { get; }
        public ICommand CancelImageCommand { get; }

        public ChatClient ChatClient { set; get; } // Сеанс
        public ClientUserInfo Client { get => client; set => Set(ref client, value); } // Вся информация о подключенном юзере

        public ObservableCollection<KeyValuePair<int, AvailableUser>> Users { get; private set; }
        public ObservableCollection<Models.Chat> Chats { get => chats; set => Set(ref chats, value); }
        public Models.Chat SelectedChat { get => selectedChat; set { if (selectedChat == null) RemoveUC.Invoke(currentView); Set(ref selectedChat, value); } }
        public MahApps.Metro.Controls.HamburgerMenuIconItem SelectedOptionsItem { get => selectedOptionsItem; set => Set(ref selectedOptionsItem, value); }
        public async void LoadedWindow(object sender)
        {
            try
            {
                AddUC.Invoke(currentView);
                client.DownloadAvatarAsync();
                await LoadChatroomsAsync();

                foreach (var item in chats)
                {
                    if (item is ChatGroup)
                        DownloadChatAvatarAsync(item as ChatGroup);
                }
            }
            catch (FaultException<ConnectionExceptionFault> ex)
            {
                new Views.DialogWindow(ex.Message).ShowDialog();
            }
            catch (IOException)
            {
                new Views.DialogWindow("avatar image could not be download").ShowDialog();
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
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
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
                new Views.DialogWindow(ex.Message).ShowDialog();
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
            if (selectedChat != null) RemoveUC.Invoke(currentView);
            Views.ChatUC chatView = new Views.ChatUC();
            ChatViewModel viewModel = new ChatViewModel(this);
            chatView.getScroll = new Views.ChatUC.GetControlDelegate(viewModel.SetScrollViewer);
            chatView.getRichTextBox = new Views.ChatUC.GetControlDelegate(viewModel.SetInputRichBox);
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

        private void SettingsShow(object param)
        {
            SelectedOptionsItem = null;
            RemoveUC.Invoke(currentView);
            SelectedChat = null;
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
                settings.RemoveMute(chatId);
            }

        }

        private Models.Chat FindChatroom(int chatId)
        {
            foreach (var chat in chats)
                if (chat.SqlId == chatId)
                    return chat;
            return null;
        }

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

                        if (item.LeaveDate != DateTime.MinValue)
                        {
                            continue;
                        }

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
            settings.RemoveMute(chatId);

            if (chats.Where(c => c.FindUser(userId) != null).ToList().Count < 1)
            {
                KeyValuePair<int, AvailableUser> availableUser = Users.FirstOrDefault(u => u.Key == userId);
                if (availableUser.Value != null)
                {
                    availableUser.Value.Image = null;
                    availableUser.Value.IsOnline = false;
                    Users.Remove(availableUser);
                }
            }
        }

        public void ReplyMessage(ServiceMessageText message, int chatroomId)
        {
            var chat = FindChatroom(chatroomId);
            chat.LastMessage = new TextMessage(message.Text, message.DateTime);

            if (Chats.IndexOf(chat) != 0)
                Chats.Move(Chats.IndexOf(chat), 0);

            if (chat.Equals(SelectedChat))
            {
                chat.Messages.Add(chat.GetMessageType(Client.SqlId, message.UserId, new TextMessage(message.Text, message.DateTime)));
                if (!mainWindow.IsVisible && settings.CanNotify)
                {
                    if (chat.CanNotify(settings))
                        settings.PlayRington(chat.GetNotifyPath(settings));
                    Notify(chat);
                }
            }
            else if (chat.CanNotify(settings))
            {
                settings.PlayRington(chat.GetNotifyPath(settings));
                if (!mainWindow.IsVisible && settings.CanNotify) Notify(chat);
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

            if (chat.Equals(SelectedChat))
            {
                chat.Messages.Add(chat.GetMessageType(Client.SqlId, serviceMessageFile.UserId, new FileMessage(serviceMessageFile.FileName, serviceMessageFile.DateTime, serviceMessageFile.StreamId)));
                if (!mainWindow.IsVisible && settings.CanNotify)
                {
                    if (chat.CanNotify(settings))
                        settings.PlayRington(chat.GetNotifyPath(settings));
                    Notify(chat);
                }
            }
            else if (chat.CanNotify(settings))
            {
                if (chat.CanNotify(settings))
                    settings.PlayRington(chat.GetNotifyPath(settings));
                if (!mainWindow.IsVisible && settings.CanNotify) Notify(chat);
            }
        }

        private async Task LoadChatroomsAsync()
        {
            Dictionary<Chatroom, UserInChat[]> chatrooms = await ChatClient.FindAllChatroomsForClientAsync(client.SqlId);
            await System.Threading.Tasks.Task.Run(async () =>
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
                        clientChatrooms.Add(new ChatGroup(key.ChatSqlId, key.ChatName, availableUsers) { CanWrite = canWrite });
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
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                user.Image = new BitmapImage(new Uri("Resources/user.png", UriKind.Relative));
                            });
                        }
                        clientChatrooms.Add(new ChatOne(key.ChatSqlId, user) { CanWrite = !friend.IsLeft });
                    }
                }

                foreach (var item in clientChatrooms)
                {
                    item.LastMessage = await Utility.MessageLoader.LoadMessage(item, client.SqlId, 1, 1);
                }
                clientChatrooms.Sort((a, b) => { return b.LastMessage.Date.CompareTo(a.LastMessage.Date); });

                App.Current.Dispatcher.Invoke(() =>
                {

                    foreach (var item in clientChatrooms)
                    {
                        Chats.Add(item);
                    }
                });
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
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            user.Image = new BitmapImage(new Uri("Resources/user.png", UriKind.Relative));
                        });
                        return;
                    }
                    memoryStream = FileHelper.ReadFileByPart(stream);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
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
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            chat.Avatar = new BitmapImage(new Uri("Resources/group.png", UriKind.Relative));
                        });
                        return;
                    }
                    memoryStream = FileHelper.ReadFileByPart(stream);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
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

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            mainWindow.Hide();
            mainWindow.Closing -= WindowClosing;
        }

        private void ExitFromTray(object sender, EventArgs e)
        {
            icon.Dispose();
            mainWindow.Closing -= WindowClosing;
            mainWindow.Close();
        }

        private void ActivateWindow(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                mainWindow.Show();
                mainWindow.Closing += WindowClosing;
            }
        }

        private void Notify(Models.Chat chat)
        {
            UserControls.NotifyUC notifyUC = new UserControls.NotifyUC();
            notifyUC.DataContext = chat;
            notifyManager.Show(notifyUC, "", new TimeSpan(0, 0, 5));
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
