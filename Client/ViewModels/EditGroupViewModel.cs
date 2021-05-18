using Client.Models;
using Client.Utility;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Client.ViewModels
{
    class EditGroupViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ChatGroup chat;
        private MainViewModel mainVM;
        private int offset = 0;

        private ObservableCollection<AvailableUser> users;
        private ObservableCollection<AvailableUser> allUsers;

        private bool canDrop;

        private string name;
        private string searchMembersText;
        private string searchUsersText;

        public EditGroupViewModel(MainViewModel mainVM)
        {
            this.mainVM = mainVM;
            ChatClient = mainVM.ChatClient;

            Chat = (ChatGroup)mainVM.SelectedChat;
            Users = new ObservableCollection<AvailableUser>(Chat.Users);
            Name = Chat.GroupName;

            RemoveMemberCommand = new Command(RemoveMember);
            DeleteChatCommand = new Command(DeleteChat);
            ChangeImageCommand = new Command(ChangeImage);
            ShowMoreCommand = new Command(ShowMore);
            SaveNameCommand = new Command(SaveName);
            SearchChangedCommand = new Command(SearchChanged);

            Users_MouseLeaveCommand = new Command(Users_MouseLeave);
            Users_PreviewDragEnterCommand = new Command(Users_PreviewDragEnter);
            Users_DragLeaveCommand = new Command(Users_DragLeave);
            Users = Chat.Users;
            allUsers = new ObservableCollection<AvailableUser>();

            Users.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(
             delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
             {
                 if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                 {
                     if (AllUsers.FirstOrDefault(u => u.SqlId == (e.OldItems[0] as AvailableUser).SqlId) == null)
                     {
                         AllUsers.Add(e.OldItems[0] as AvailableUser);
                     }
                 }
             });
        }

        public ICommand RemoveMemberCommand { get; }
        public ICommand DeleteChatCommand { get; }
        public ICommand ChangeImageCommand { get; }
        public ICommand ShowMoreCommand { get; }
        public ICommand SaveNameCommand { get; }
        public ICommand SearchChangedCommand { get; }

        public ICommand Users_MouseLeaveCommand { get; }
        public ICommand Users_PreviewDragEnterCommand { get; }
        public ICommand Users_DragLeaveCommand { get; }

        public ChatService.ChatClient ChatClient { get; set; }

        public string Name { get => name; set => Set(ref name, value); }
        public string SearchMembersText { get => searchMembersText; set => Set(ref searchMembersText, value); }
        public string SearchUsersText { get => searchUsersText; set => Set(ref searchUsersText, value); }

        public ObservableCollection<AvailableUser> Users { get => users; set => Set(ref users, value); }
        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }
        public ChatGroup Chat { get => chat; set => Set(ref chat, value); }

        private void RemoveMember(object param)
        {
            AvailableUser user = (AvailableUser)param;
            mainVM.SelectedChat.RemoveUser(user);
            ChatClient.RemoveUserFromChatroom(user.SqlId, chat.SqlId);
        }

        private void DeleteChat(object param)
        {
            //mainVM.Chats.Remove(mainVM.SelectedChat);
        }

        private void SaveName(object param)
        {
            if (Name != null)
            {
                //функция для изменения имени чата в сервере
                chat.GroupName = name;
            }
        }

        private void SearchChanged(object param)
        {
            Users.Clear();
            if (SearchMembersText == null || SearchMembersText == "")
            {
                Users = new ObservableCollection<AvailableUser>(Chat.Users);
            }
            else
            {
                foreach (var user in Chat.Users)
                {
                    if (user.Name.Contains(searchMembersText))
                        Users.Add(user);
                }
            }
        }

        private void ChangeImage(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files(*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";
            if (dialog.ShowDialog() == true)
            {
                //тут метод сохранения нового фото на сервере
                BitmapImage image = new BitmapImage(new Uri(dialog.FileName));
                chat.Image = image;
            }
        }

        private void ShowMore(object param)
        {
            ChatService.UnitClient unitClient = new ChatService.UnitClient();
            Dictionary<int, string> users = unitClient.GetRegisteredUsers(15, offset, mainVM.Client.SqlId);

            if (users.Count == 0)
                return;

            var it = users.GetEnumerator();
            for (int i = 0; i < users.Count; i++)
            {
                it.MoveNext();
                if (Users.FirstOrDefault(u => u.SqlId == it.Current.Key) == null && AllUsers.FirstOrDefault(u => u.SqlId == it.Current.Key) == null)
                {
                    AllUsers.Add(new AvailableUser(it.Current.Key, it.Current.Value));
                    LoadUserAvatarAsync();
                }
            }
            offset += 15;
        }

        public void Users_MouseLeave(object param)
        {
            canDrop = false;
        }

        public void Users_PreviewDragEnter(object param)
        {
            canDrop = true;
        }

        public void Users_DragLeave(object param)
        {
            canDrop = false;
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
                try
                {
                    ChatClient.AddUserToChatroom(sourceItem.SqlId, chat.SqlId);
                    AllUsers.Remove(sourceItem);
                    Users.Add(sourceItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }

        private async void LoadUserAvatarAsync()
        {
            AvailableUser availableUser = AllUsers[AllUsers.Count - 1];
            ChatService.DownloadRequest downloadRequest = new ChatService.DownloadRequest(availableUser.SqlId);
            System.IO.Stream stream = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                var avatarClient = new ChatService.AvatarClient();
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
            catch (System.ServiceModel.FaultException<ChatService.ConnectionExceptionFault> ex)
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
