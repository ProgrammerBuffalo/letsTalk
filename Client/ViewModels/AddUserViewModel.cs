using Client.ChatService;
using Client.Models;
using Client.Utility;
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

        public delegate void AddChatDelegate(Models.Chat chat);

        public AddChatDelegate AddChat { get; set; }

        protected ClientUserInfo client;
        protected ChatClient chatClient;

        private ObservableCollection<AvailableUser> allUsers;
        private AvailableUser selectedUser;
        private string chatName;

        private int offset;
        private int count;

        public AddUserViewModel()
        {
            client = ClientUserInfo.getInstance();

            SearchChangedCommand = new Command(SearchChanged);
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
        public ICommand SearchChangedCommand { get; }
        public ICommand AddUserCommand { get; }

        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }
        public AvailableUser SelectedUser { get => selectedUser; set => Set(ref selectedUser, value); }
        public string ChatName { get => chatName; set => Set(ref chatName, value); }

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

        public void SearchChanged(object param)
        {
            string name = param.ToString();
            if (!String.IsNullOrWhiteSpace(name))
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
                    chatId = chatClient.CreateChatroom(chatName, new int[] { client.SqlId, selectedUser.SqlId });
                    chatClient.AddUserToChatroom(client.SqlId, chatId);
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
                    avatarClient.AvatarDownload(downloadRequest.Requested_UserSqlId, out lenght, out stream);
                    if (lenght != 0)
                    {
                        memoryStream = FileHelper.ReadFileByPart(stream);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
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

    class CreateGroupViewModel : AddUserViewModel
    {
        private ObservableCollection<AvailableUser> usersToAdd;

        public CreateGroupViewModel(ChatClient chatClient) : base(chatClient)
        {
            AddToGroupCommand = new Command(AddToGroup);
            RemoveFromGroupCommand = new Command(RemoveFromGroup);
            CreateGroupCommand = new Command(CreateGroup);

            UsersToAdd = new ObservableCollection<AvailableUser>();
        }

        public ICommand AddToGroupCommand { get; }
        public ICommand RemoveFromGroupCommand { get; }
        public ICommand CreateGroupCommand { get; }

        public ObservableCollection<AvailableUser> UsersToAdd { get => usersToAdd; set => Set(ref usersToAdd, value); }

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
    }
}
