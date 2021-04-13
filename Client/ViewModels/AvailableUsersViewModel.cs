using Client.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Client.Models;

namespace Client.ViewModels
{

    public class AvailableUsersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<AvailableUser> users = new ObservableCollection<AvailableUser>(); // Для загрузки пользователей из БД
        private ObservableCollection<AvailableUser> selectedUsers = new ObservableCollection<AvailableUser>(); // Выбранные пользователи, которые будут добавлены в чатрум

        private string chatroomName;

        public string ChatroomName { get => chatroomName; set => Set(ref chatroomName, value); } // Работает в связке с TextBox

        public ObservableCollection<AvailableUser> Users { get => users; set => Set(ref users, value); }
        public ObservableCollection<AvailableUser> SelectedUsers { get => selectedUsers; set => Set(ref selectedUsers, value); }

        private int count = 10; // Добавление 10 пользователей при клике More...
        private int offset = 0; // Смещение от начала записей в таблице

        public ICommand ShowMoreCommand { get; }
        public ICommand AddToGroupCommand { get; }
        public ICommand CreateChatroomCommand { get; }
        public ICommand RemoveFromGroupCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }

        public AvailableUsersViewModel()
        {
            ShowMoreCommand = new Command(ShowMoreUsers);
            AddToGroupCommand = new Command(AddToGroup);
            CreateChatroomCommand = new Command(CreateChatroom);
            RemoveFromGroupCommand = new Command(RemoveFromGroup);
        }

        public void CreateChatroom(object sender)
        {
            ChatService.ChatClient client = ClientUserInfo.getInstance().ChatClient;
            client.CreateChatroom(chatroomName, selectedUsers.Select(u => u.SqlId).ToArray());
        }

        // Добавление пользователя в группу, который в дальнейшем может стать потенциальным участником чата 
        public void AddToGroup(object sender)
        {
            if (sender == null)
                return; 
            AvailableUser availableUser = users.Where(u => u.SqlId == int.Parse(sender.ToString())).FirstOrDefault();
            selectedUsers.Add(availableUser);
            users.Remove(availableUser);
        }

        // Удаление из группы пользователя, обратное его возвращение к листБоксу доступных пользователей
        public void RemoveFromGroup(object sender)
        {
            if (sender == null)
                return;
            AvailableUser selectedUser = selectedUsers.Where(u => u.SqlId == int.Parse(sender.ToString())).FirstOrDefault();
            selectedUsers.Remove(selectedUser);
            users.Add(selectedUser);
            users.Move(users.Count - 1, users.IndexOf(users.Where(u => u.SqlId > selectedUser.SqlId).FirstOrDefault()));
        }

        // Подгрузка пользователей 
        public void ShowMoreUsers(object sender)
        {
            ClientUserInfo client = ClientUserInfo.getInstance();
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
            keyValuePairs = client.ChatClient.GetUsers(count, offset, client.SqlId);

            foreach (var item in keyValuePairs)
            {
                Users.Add(new AvailableUser { SqlId = item.Key, Name = item.Value });
            }

            for (int i = 0; i < users.Count; i++)
                LoadUserAvatarAsync(i);
            offset += 10;
        }

        // Загрузка аватарок пользователей
        private async void LoadUserAvatarAsync(int index)
        {
            ChatService.DownloadRequest downloadRequest = new ChatService.DownloadRequest(users[index].SqlId);
            System.IO.Stream stream = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                var fileClient = new ChatService.FileClient();
                long lenght;

                await System.Threading.Tasks.Task.Run(() =>
                {
                    fileClient.AvatarDownload(downloadRequest.Requested_UserSqlId, out lenght, out stream);
                    if (lenght != 0)
                    {
                        memoryStream = FileHelper.ReadFileByPart(stream);

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = memoryStream;
                            bitmapImage.EndInit();

                            users[index].Image = bitmapImage;                         
                        });
                    }
                });
            }
            catch (FaultException<ChatService.ConnectionExceptionFault> ex)
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
    }

}
