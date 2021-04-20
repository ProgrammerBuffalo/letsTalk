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
    class AddUserViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ClientUserInfo client;
        private ChatClient chatClient;

        private ObservableCollection<AvailableUser> allUsers;

        private int offset;
        private int count;

        public AddUserViewModel(ChatClient chatClient)
        {
            this.chatClient = chatClient;
            client = ClientUserInfo.getInstance();

            SearchChangedCommand = new Command(SearchChanged);
            ShowMoreCommand = new Command(ShowMore);
            AddUserCommand = new Command(AddUser);

            AllUsers = new ObservableCollection<AvailableUser>();

            offset = 10;
            count = 0;
        }

        public ICommand ShowMoreCommand { get; }
        public ICommand SearchChangedCommand { get; }
        public ICommand AddUserCommand { get; }

        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }

        public void ShowMore(object param)
        {
            Dictionary<int, string> users = chatClient.GetRegisteredUsers(count, offset, client.SqlId);

            var it = users.GetEnumerator();
            for (int i = 0; i < users.Count; i++)
            {
                it.MoveNext();
                AllUsers.Add(new AvailableUser(it.Current.Value, it.Current.Key));
                LoadUserAvatarAsync(i);
            }
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


        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }

    class CreateGroupViewModel : AddUserViewModel
    {
        public CreateGroupViewModel(ChatClient chatClient) : base(chatClient)
        {

        }

        ICommand AddToGroupCommand { get; }
        ICommand RemoveFromGroupCommand { get; }
        ICommand CreateGroupCommand { get; }

        private void AddToGroup(object param)
        {

        }

        private void RemoveFromGroup(object param)
        {

        }

        private void CreateGroup(object param)
        {

        }
    }
}
