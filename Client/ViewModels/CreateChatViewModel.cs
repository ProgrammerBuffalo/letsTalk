using Client.ChatService;
using Client.Models;
using Client.Utility;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Client.ViewModels
{
    //пока что сделал все в одном потоке потом исправлю шас на это времяни нет
    class CreateChatViewModel : System.ComponentModel.INotifyPropertyChanged, IDropTarget
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private MainViewModel mainVM;

        private bool canDrop;
        private bool allUsersIsDropSource;
        private bool usersToAddIsDropSource;
        private bool allUsersIsDropTarget;
        private bool usersToAddIsDropTarget;

        private string searchText;
        private string chatName;
        private AvailableUser selectedUser;
        private ObservableCollection<AvailableUser> allUsers;
        private ObservableCollection<AvailableUser> usersToAdd;

        //public MainViewModel.ChatDelegate AddChat { get; set; }

        //private ClientUserInfo client;
        //private ChatClient chatClient;

        private int offset;
        private int count;

        public CreateChatViewModel(MainViewModel mainVM)
        {
            this.mainVM = mainVM;
            //this.client = client;
            //this.chatClient = chatClient;

            AllUsers = new ObservableCollection<AvailableUser>();
            UsersToAdd = new ObservableCollection<AvailableUser>();

            SearchCommand = new Command(Search);
            ShowMoreCommand = new Command(ShowMore);
            CreateGroupCommand = new Command(CreateGroup);

            AllUsers_MouseLeaveCommand = new Command(AllUsers_MouseLeave);
            AllUsers_PreviewDragEnterCommand = new Command(AllUsers_PreviewDragEnter);
            AllUsers_DragLeaveCommand = new Command(AllUsers_DragLeave);
            AllUsers_MouseEnterCommand = new Command(AllUsers_MouseEnter);

            UsersToAdd_MouseLeaveCommand = new Command(UsersToadd_MouseLeave);
            UsersToAdd_PreviewDragEnterCommand = new Command(UsersToAdd_PreviewDragEnter);
            UsersToAdd_DragLeaveCommand = new Command(UsersToAdd_DragLeave);
            UsersToAdd_MouseEnterCommand = new Command(UsersToAdd_MouseEnter);

            offset = 0;
            count = 10;
        }

        public ICommand ShowMoreCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateGroupCommand { get; }

        public ICommand AllUsers_MouseLeaveCommand { get; }
        public ICommand AllUsers_PreviewDragEnterCommand { get; }
        public ICommand AllUsers_DragLeaveCommand { get; }
        public ICommand AllUsers_MouseEnterCommand { get; }

        public ICommand UsersToAdd_MouseLeaveCommand { get; }
        public ICommand UsersToAdd_PreviewDragEnterCommand { get; }
        public ICommand UsersToAdd_DragLeaveCommand { get; }
        public ICommand UsersToAdd_MouseEnterCommand { get; }

        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }
        public ObservableCollection<AvailableUser> UsersToAdd { get => usersToAdd; set => Set(ref usersToAdd, value); }
        public AvailableUser SelectedUser { get => selectedUser; set => Set(ref selectedUser, value); }

        public string ChatName { get => chatName; set => Set(ref chatName, value); }
        public string SearchText { get => searchText; set => Set(ref searchText, value); }

        public bool AllUsersIsDropTarget { get => allUsersIsDropTarget; set => Set(ref allUsersIsDropTarget, value); }
        public bool UsersToAddIsDropTarget { get => usersToAddIsDropTarget; set => Set(ref usersToAddIsDropTarget, value); }

        public void ShowMore(object param)
        {
            UnitClient unitClient = new UnitClient();
            Dictionary<int, string> users = unitClient.GetRegisteredUsers(count, offset, mainVM.Client.SqlId);

            if (users.Count == 0)
                return;

            var it = users.GetEnumerator();
            for (int i = 0; i < users.Count; i++)
            {
                it.MoveNext();
                AllUsers.Add(new AvailableUser(it.Current.Key, it.Current.Value));
                LoadUserAvatarAsync();
            }
            offset += count;
        }

        //метод для поиска новых пользователй (использовать SearchText для поиска по имени)
        public void Search(object param)
        {
            //if (!String.IsNullOrWhiteSpace(searchText))
            //{
            //    AllUsers.Clear();
            //}
        }

        private void CreateGroup(object param)
        {
            int sqlId;
            Models.Chat chat;
            if (usersToAdd.Count == 1)
            {
                try
                {
                    sqlId = mainVM.ChatClient.CreateChatroom(new int[] { mainVM.Client.SqlId, usersToAdd[0].SqlId }, "");

                    AvailableUser user = mainVM.Users.FirstOrDefault(u => u.Key == usersToAdd[0].SqlId).Value;
                    if (user == null)
                    {
                        user = new AvailableUser(usersToAdd[0].SqlId, usersToAdd[0].Name);
                        mainVM.Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                    }

                    chat = new ChatOne(sqlId, usersToAdd[0]) { CanWrite = true };
                    mainVM.Chats.Add(chat);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(ChatName))
                {
                    int[] users = new int[usersToAdd.Count + 1];
                    users[0] = mainVM.Client.SqlId;
                    for (int i = 1; i < users.Length; i++)
                        users[i] = usersToAdd[i - 1].SqlId;

                    try
                    {
                        sqlId = mainVM.ChatClient.CreateChatroom(users, ChatName);
                        List<AvailableUser> availableUsers = new List<AvailableUser>();
                        foreach (var item in usersToAdd)
                        {
                            AvailableUser user = mainVM.Users.FirstOrDefault(u => u.Key == item.SqlId).Value;
                            if (user == null)
                            {
                                user = new AvailableUser(item.SqlId, item.Name);
                                mainVM.Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                            }
                            availableUsers.Add(user);
                        }
                        BitmapImage image = new BitmapImage(new Uri("Resources/group.png", UriKind.Relative));
                        chat = new ChatGroup(sqlId, ChatName, availableUsers) { CanWrite = true, Avatar = image };
                        mainVM.Chats.Add(chat);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter chatroom name");
                }
            }
        }

        private void UsersToAdd_PreviewDragEnter(object param)
        {
            canDrop = true;
            allUsersIsDropSource = true;
        }

        private void UsersToAdd_DragLeave(object param)
        {
            canDrop = false;
            allUsersIsDropSource = false;
        }

        private void UsersToadd_MouseLeave(object param)
        {
            canDrop = false;
            allUsersIsDropSource = false;
        }

        private void AllUsers_PreviewDragEnter(object param)
        {
            canDrop = true;
            usersToAddIsDropSource = true;
        }

        private void AllUsers_DragLeave(object param)
        {
            canDrop = false;
            usersToAddIsDropSource = false;
        }

        private void AllUsers_MouseLeave(object param)
        {
            canDrop = false;
            usersToAddIsDropSource = false;
        }

        private void AllUsers_MouseEnter(object param)
        {
            AllUsersIsDropTarget = false;
            UsersToAddIsDropTarget = true;
        }

        private void UsersToAdd_MouseEnter(object param)
        {
            UsersToAddIsDropTarget = false;
            AllUsersIsDropTarget = true;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            AvailableUser sourceItem = dropInfo.Data as AvailableUser;
            if (sourceItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            AvailableUser sourceItem = dropInfo.Data as AvailableUser;
            if (sourceItem != null && canDrop)
            {
                if (allUsersIsDropSource)
                {
                    usersToAdd.Add(sourceItem);
                    AllUsers.Remove(sourceItem);
                }
                else if (usersToAddIsDropSource)
                {
                    AllUsers.Add(sourceItem);
                    UsersToAdd.Remove(sourceItem);
                }
            }
        }

        private async void LoadUserAvatarAsync()
        {
            AvailableUser availableUser = AllUsers[AllUsers.Count - 1];
            DownloadRequest downloadRequest = new DownloadRequest(availableUser.SqlId);
            System.IO.Stream stream = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                var avatarClient = new AvatarClient();
                long lenght;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    avatarClient.UserAvatarDownload(downloadRequest.Requested_SqlId, out lenght, out stream);
                    if (lenght > 0)
                    {
                        memoryStream = FileHelper.ReadFileByPart(stream);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var bitmapImage = new BitmapImage();

                            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                            bitmapImage.BeginInit();
                            bitmapImage.DecodePixelWidth = 400;
                            bitmapImage.DecodePixelHeight = 400;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();

                            availableUser.Image = bitmapImage;
                        });
                    }
                });
            }
            catch (FaultException<ConnectionExceptionFault> ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }

    //class CreateGroupViewModel : CreateChatViewModel, IDropTarget
    //{


    //    public CreateGroupViewModel(ClientUserInfo client, ChatClient chatClient) : base(client)
    //    {
    //        AddToGroupCommand = new Command(AddToGroup);
    //        RemoveFromGroupCommand = new Command(RemoveFromGroup);
    //        CreateGroupCommand = new Command(CreateGroup);

    //        AllUsers_MouseLeaveCommand = new Command(AllUsers_MouseLeave);
    //        AllUsers_PreviewDragEnterCommand = new Command(AllUsers_PreviewDragEnter);
    //        AllUsers_DragLeaveCommand = new Command(AllUsers_DragLeave);
    //        AllUsers_MouseEnterCommand = new Command(AllUsers_MouseEnter);

    //        UsersToAdd_MouseLeaveCommand = new Command(UsersToadd_MouseLeave);
    //        UsersToAdd_PreviewDragEnterCommand = new Command(UsersToAdd_PreviewDragEnter);
    //        UsersToAdd_DragLeaveCommand = new Command(UsersToAdd_DragLeave);
    //        UsersToAdd_MouseEnterCommand = new Command(UsersToAdd_MouseEnter);

    //        UsersToAdd = new ObservableCollection<AvailableUser>();
    //    }

    //    public ICommand AddToGroupCommand { get; }
    //    public ICommand RemoveFromGroupCommand { get; }
    //    public ICommand CreateGroupCommand { get; }

    //    public ICommand AllUsers_MouseLeaveCommand { get; }
    //    public ICommand AllUsers_PreviewDragEnterCommand { get; }
    //    public ICommand AllUsers_DragLeaveCommand { get; }
    //    public ICommand AllUsers_MouseEnterCommand { get; }

    //    public ICommand UsersToAdd_MouseLeaveCommand { get; }
    //    public ICommand UsersToAdd_PreviewDragEnterCommand { get; }
    //    public ICommand UsersToAdd_DragLeaveCommand { get; }
    //    public ICommand UsersToAdd_MouseEnterCommand { get; }

    //    public ObservableCollection<AvailableUser> UsersToAdd { get => usersToAdd; set => Set(ref usersToAdd, value); }
    //    public string ChatName { get => chatName; set => Set(ref chatName, value); }
    //    public bool AllUsersIsDropTarget { get => allUsersIsDropTarget; set => Set(ref allUsersIsDropTarget, value); }
    //    public bool UsersToAddIsDropTarget { get => usersToAddIsDropTarget; set => Set(ref usersToAddIsDropTarget, value); }

    //    //private void AddToGroup(object param)
    //    //{
    //    //    if (SelectedUser != null)
    //    //    {
    //    //        usersToAdd.Add(SelectedUser);
    //    //        AllUsers.Remove(SelectedUser);
    //    //    }
    //    //}

    //    //private void RemoveFromGroup(object param)
    //    //{
    //    //    if (SelectedUser != null)
    //    //    {
    //    //        AllUsers.Add(SelectedUser);
    //    //        usersToAdd.Remove(SelectedUser);
    //    //    }
    //    //}

    //    private void CreateGroup(object param)
    //    {
    //        int sqlId;
    //        Chat chat;
    //        if (usersToAdd.Count == 1)
    //        {
    //            sqlId = chatClient.CreateChatroom(usersToAdd[0].Name, new int[] { client.SqlId, usersToAdd[0].SqlId });
    //            chat = new ChatOne(sqlId, usersToAdd[0]) { CanWrite = true };
    //            AddChat.Invoke(chat);
    //        }
    //        else
    //        {
    //            if (!String.IsNullOrWhiteSpace(ChatName))
    //            {
    //                int[] users = new int[usersToAdd.Count + 1];
    //                users[0] = client.SqlId;
    //                for (int i = 1; i < users.Length; i++)
    //                    users[i] = usersToAdd[i - 1].SqlId;

    //                sqlId = chatClient.CreateChatroom(ChatName, users);

    //                chat = new ChatGroup(sqlId, ChatName, UsersToAdd) { CanWrite = true };
    //                AddChat.Invoke(chat);
    //            }
    //            else
    //            {
    //                MessageBox.Show("Please enter chat room name");
    //            }
    //        }
    //    }

    //    private void UsersToAdd_PreviewDragEnter(object param)
    //    {
    //        canDrop = true;
    //        allUsersIsDropSource = true;
    //    }

    //    private void UsersToAdd_DragLeave(object param)
    //    {
    //        canDrop = false;
    //        allUsersIsDropSource = false;
    //    }

    //    private void UsersToadd_MouseLeave(object param)
    //    {
    //        canDrop = false;
    //        allUsersIsDropSource = false;
    //    }

    //    private void AllUsers_PreviewDragEnter(object param)
    //    {
    //        canDrop = true;
    //        usersToAddIsDropSource = true;
    //    }

    //    private void AllUsers_DragLeave(object param)
    //    {
    //        canDrop = false;
    //        usersToAddIsDropSource = false;
    //    }

    //    private void AllUsers_MouseLeave(object param)
    //    {
    //        canDrop = false;
    //        usersToAddIsDropSource = false;
    //    }

    //    private void AllUsers_MouseEnter(object param)
    //    {
    //        AllUsersIsDropTarget = false;
    //        UsersToAddIsDropTarget = true;
    //    }

    //    private void UsersToAdd_MouseEnter(object param)
    //    {
    //        UsersToAddIsDropTarget = false;
    //        AllUsersIsDropTarget = true;
    //    }

    //    public void DragOver(IDropInfo dropInfo)
    //    {
    //        AvailableUser sourceItem = dropInfo.Data as AvailableUser;
    //        if (sourceItem != null)
    //        {
    //            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
    //            dropInfo.Effects = DragDropEffects.Copy;
    //        }
    //    }

    //    public void Drop(IDropInfo dropInfo)
    //    {
    //        AvailableUser sourceItem = dropInfo.Data as AvailableUser;
    //        if (sourceItem != null && canDrop)
    //        {
    //            if (allUsersIsDropSource)
    //            {
    //                usersToAdd.Add(sourceItem);
    //                AllUsers.Remove(sourceItem);
    //            }
    //            else if (usersToAddIsDropSource)
    //            {
    //                AllUsers.Add(sourceItem);
    //                UsersToAdd.Remove(sourceItem);
    //            }
    //        }
    //    }
    //}



    //public void AddUser(object param)
    //{
    //    if (selectedUser != null)
    //    {
    //        int chatId;
    //        try
    //        {
    //            chatId = chatClient.CreateChatroom(selectedUser.Name, new int[] { client.SqlId, selectedUser.SqlId });
    //            //chatClient.AddUserToChatroom(client.SqlId, chatId);
    //        }
    //        catch (Exception)
    //        {
    //            MessageBox.Show("someting went wrong please try later");
    //            return;
    //        }
    //        ChatOne chat = new ChatOne(chatId, selectedUser);
    //        AddChat.Invoke(chat);
    //        MessageBox.Show("friend added");
    //    }
    //    else
    //    {
    //        MessageBox.Show("Select user for creating chat room");
    //    }
    //}
}
