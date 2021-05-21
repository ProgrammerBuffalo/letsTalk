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
            ChangeImageCommand = new Command(ChangeImageAsync);
            CancelImageCommand = new Command(CancelImage);
            ShowMoreCommand = new Command(ShowMore);
            SaveNameCommand = new Command(SaveName);
            SearchChangedCommand = new Command(SearchChanged);
            TextBoxSearch_EnterPressedCommand = new Command(SearchUsersFromDB);

            Users_MouseLeaveCommand = new Command(Users_MouseLeave);
            Users_PreviewDragEnterCommand = new Command(Users_PreviewDragEnter);
            Users_DragLeaveCommand = new Command(Users_DragLeave);
            Users = Chat.Users;
            allUsers = new ObservableCollection<AvailableUser>();

        }

        public ICommand RemoveMemberCommand { get; }
        public ICommand DeleteChatCommand { get; }
        public ICommand ChangeImageCommand { get; }
        public ICommand CancelImageCommand { get; }
        public ICommand ShowMoreCommand { get; }
        public ICommand SaveNameCommand { get; }
        public ICommand SearchChangedCommand { get; }
        public ICommand TextBoxSearch_EnterPressedCommand { get; }

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
            if (SearchMembersText == "" || searchMembersText == null)
                return;
            List<AvailableUser> sortedUsers = new List<AvailableUser>();


            for (int i = 0; i < Users.Count; i++)
            {
                sortedUsers.AddRange(Users.Where(u => u.Name.Substring(0, SearchMembersText.Length <= u.Name.Length ? SearchMembersText.Length : 0).Contains(searchMembersText)).ToList());
            }

            if (sortedUsers.Count < 1)
                return;

            for (int i = 0; i < sortedUsers.Count; i++)
            {
                Users.Move(Users.IndexOf(Users.First(u => u.SqlId == sortedUsers[i].SqlId)), 0);
            }
        }

        private void SearchUsersFromDB(object obj)
        {
            offset = 0;

            ChatService.UnitClient unitClient = new ChatService.UnitClient();
            Dictionary<int, string> users = unitClient.SearchUsersByName(SearchUsersText);

            if (users.Count == 0)
                return;

            var it = users.GetEnumerator();
            for (int i = 0; i < users.Count; i++)
            {
                it.MoveNext();
                if (Users.FirstOrDefault(u => u.SqlId == it.Current.Key) == null && AllUsers.FirstOrDefault(u => u.SqlId == it.Current.Key) == null)
                {
                    AvailableUser availableUser = new AvailableUser(it.Current.Key, it.Current.Value);
                    AllUsers.Add(availableUser);
                    mainVM.DownloadUserAvatarAsync(availableUser);
                }
            }
        }

        private async void ChangeImageAsync(object param)
        {
            ChatService.UploadFileInfo uploadFileInfo = null;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == true)
                {

                    System.IO.MemoryStream memoryStream = ImageCropper.GetCroppedImage(openFileDialog.FileName);
                    memoryStream.Position = 0;

                    uploadFileInfo = new ChatService.UploadFileInfo { FileName = openFileDialog.FileName, FileStream = memoryStream, Responsed_SqlId = chat.SqlId };
                    ChatService.AvatarClient avatarClient = new ChatService.AvatarClient();

                    if (uploadFileInfo.FileStream.CanRead)
                        await avatarClient.ChatAvatarUploadAsync(uploadFileInfo.FileName, uploadFileInfo.Responsed_SqlId, uploadFileInfo.FileStream);

                    memoryStream.Position = 0;

                    var bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    chat.Avatar = bitmap;
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

        private void CancelImage(object param)
        {
            chat.Avatar = new BitmapImage(new Uri("Resources/group.png", UriKind.Relative));
            ChatService.UnitClient unitClient = new ChatService.UnitClient();
            unitClient.ChatAvatarDeleteAsync(chat.SqlId);
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
                    AvailableUser availableUser = new AvailableUser(it.Current.Key, it.Current.Value);
                    AllUsers.Add(availableUser);
                    mainVM.DownloadUserAvatarAsync(availableUser);
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

                    AvailableUser user = mainVM.Users.FirstOrDefault(u => u.Key == sourceItem.SqlId).Value;
                    if (user == null)
                    {
                        user = new AvailableUser(sourceItem.SqlId, sourceItem.Name);
                        mainVM.Users.Add(new KeyValuePair<int, AvailableUser>(user.SqlId, user));
                    }

                    this.Chat.AddMember(user);
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
    }
}
