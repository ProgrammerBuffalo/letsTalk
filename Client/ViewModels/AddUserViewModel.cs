using Client.ChatService;
using Client.Models;
using Client.Utility;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Client.ViewModels
{
    //пока что сделал все в одном потоке потом исправлю шас на это времяни нет
    class AddUserViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public MainViewModel.ChatDelegate AddChat { get; set; }

        protected ClientUserInfo client;
        protected ChatClient chatClient;

        private ObservableCollection<AvailableUser> allUsers;
        private AvailableUser selectedUser;

        private int offset;
        private int count;

        private string searchText;

        public AddUserViewModel()
        {
            client = ClientUserInfo.getInstance();

            SearchCommand = new Command(Search);
            ShowMoreCommand = new Command(ShowMore);
            AddUserCommand = new Command(AddUser);

            AllUsers = new ObservableCollection<AvailableUser>();

            offset = 0;
            count = 10;
        }

        public AddUserViewModel(ChatClient chatClient) : this()
        {
            this.chatClient = chatClient;
        }

        public ICommand ShowMoreCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand SearchCommand { get; }

        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }
        public AvailableUser SelectedUser { get => selectedUser; set => Set(ref selectedUser, value); }
        public string SearchText { get => searchText; set => Set(ref searchText, value); }

        public void ShowMore(object param)
        {
            UnitClient unitClient = new UnitClient();
            Dictionary<int, string> users = unitClient.GetRegisteredUsers(count, offset, client.SqlId);

            var it = users.GetEnumerator();
            for (int i = 0; i < users.Count; i++)
            {
                it.MoveNext();
                AllUsers.Add(new AvailableUser(it.Current.Value, it.Current.Key));
                LoadUserAvatarAsync(i);
            }
            offset += count;
        }

        //метод для поиска новых пользователй (использовать SearchText для поиска по имени)
        public void Search(object param)
        {
            if (!String.IsNullOrWhiteSpace(searchText))
            {
                AllUsers.Clear();
            }
        }

        public void AddUser(object param)
        {
            if (selectedUser != null)
            {
                int chatId;
                try
                {
                    chatId = chatClient.CreateChatroom(selectedUser.Name, new int[] { client.SqlId, selectedUser.SqlId });
                    //chatClient.AddUserToChatroom(client.SqlId, chatId);
                }
                catch (Exception)
                {
                    MessageBox.Show("someting went wrong please try later");
                    return;
                }
                ChatOne chat = new ChatOne(chatId, selectedUser);
                AddChat.Invoke(chat);
                MessageBox.Show("friend added");
            }
            else
            {
                MessageBox.Show("Select user for creating chat room");
            }
        }

        private async void LoadUserAvatarAsync(int index)
        {
            DownloadRequest downloadRequest = new DownloadRequest(AllUsers[index].SqlId);
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

                            AllUsers[index].Image = bitmapImage;
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

    class CreateGroupViewModel : AddUserViewModel, IDropTarget
    {
        private ObservableCollection<AvailableUser> usersToAdd;
        private string chatName;

        private bool canDrop;
        private bool allUsersIsDropSource;
        private bool usersToAddIsDropSource;
        private bool allUsersIsDropTarget;
        private bool usersToAddIsDropTarget;

        public CreateGroupViewModel(ChatClient chatClient) : base(chatClient)
        {
            AddToGroupCommand = new Command(AddToGroup);
            RemoveFromGroupCommand = new Command(RemoveFromGroup);
            CreateGroupCommand = new Command(CreateGroup);

            AllUsers_MouseLeaveCommand = new Command(AllUsers_MouseLeave);
            AllUsers_PreviewDragEnterCommand = new Command(AllUsers_PreviewDragEnter);
            AllUsers_DragLeaveCommand = new Command(AllUsers_DragLeave);
            AllUsers_MouseEnterCommand = new Command(AllUsers_MouseEnter);

            UsersToAdd_MouseLeaveCommand = new Command(UsersToadd_MouseLeave);
            UsersToAdd_PreviewDragEnterCommand = new Command(UsersToAdd_PreviewDragEnter);
            UsersToAdd_DragLeaveCommand = new Command(UsersToAdd_DragLeave);
            UsersToAdd_MouseEnterCommand = new Command(UsersToAdd_MouseEnter);

            UsersToAdd = new ObservableCollection<AvailableUser>();
        }

        public ICommand AddToGroupCommand { get; }
        public ICommand RemoveFromGroupCommand { get; }
        public ICommand CreateGroupCommand { get; }

        public ICommand AllUsers_MouseLeaveCommand { get; }
        public ICommand AllUsers_PreviewDragEnterCommand { get; }
        public ICommand AllUsers_DragLeaveCommand { get; }
        public ICommand AllUsers_MouseEnterCommand { get; }

        public ICommand UsersToAdd_MouseLeaveCommand { get; }
        public ICommand UsersToAdd_PreviewDragEnterCommand { get; }
        public ICommand UsersToAdd_DragLeaveCommand { get; }
        public ICommand UsersToAdd_MouseEnterCommand { get; }

        public ObservableCollection<AvailableUser> UsersToAdd { get => usersToAdd; set => Set(ref usersToAdd, value); }
        public string ChatName { get => chatName; set => Set(ref chatName, value); }
        public bool AllUsersIsDropTarget { get => allUsersIsDropTarget; set => Set(ref allUsersIsDropTarget, value); }
        public bool UsersToAddIsDropTarget { get => usersToAddIsDropTarget; set => Set(ref usersToAddIsDropTarget, value); }

        private void AddToGroup(object param)
        {
            if (SelectedUser != null)
            {
                usersToAdd.Add(SelectedUser);
                AllUsers.Remove(SelectedUser);
            }
        }

        private void RemoveFromGroup(object param)
        {
            if (SelectedUser != null)
            {
                AllUsers.Add(SelectedUser);
                usersToAdd.Remove(SelectedUser);
            }
        }

        private void CreateGroup(object param)
        {
            if (!String.IsNullOrWhiteSpace(ChatName))
            {
                int[] users = new int[usersToAdd.Count + 1];
                users[0] = client.SqlId;
                for (int i = 1; i < users.Length; i++)
                    users[i] = usersToAdd[i - 1].SqlId;

                int sqlId = chatClient.CreateChatroom(ChatName, users);

                ChatGroup group = new ChatGroup(sqlId, ChatName, UsersToAdd);
                AddChat.Invoke(group);
            }
            else
            {
                MessageBox.Show("Please enter chat room name");
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
    }
}
