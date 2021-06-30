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
    class CreateChatViewModel : System.ComponentModel.INotifyPropertyChanged, IDropTarget
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private MainViewModel mainVM;

        private bool canDrop;
        private bool allUsersIsDropSource;
        private bool usersToAddIsDropSource;
        private bool allUsersIsDropTarget;
        private bool usersToAddIsDropTarget;

        private string searchText = "";
        private string acceptedText = "";
        private string chatName;
        private AvailableUser selectedUser;
        private ObservableCollection<AvailableUser> allUsers;
        private ObservableCollection<AvailableUser> usersToAdd;

        private int offset;

        public CreateChatViewModel(MainViewModel mainVM)
        {
            this.mainVM = mainVM;

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
            Dictionary<int, int> users = unitClient.GetRegisteredUsers(5, offset, mainVM.Client.SqlId, acceptedText);

            if (users.Count == 0)
                return;

            var it = users.GetEnumerator();
            for (int i = 0; i < users.Count; i++)
            {
                it.MoveNext();
                AvailableUser availableUser = new AvailableUser(it.Current.Key, unitClient.FindUserName(it.Current.Value));
                AllUsers.Add(availableUser);
                mainVM.DownloadUserAvatarAsync(availableUser);
            }
            offset = users.Last().Value;
        }

        public void Search(object param)
        {
            AllUsers.Clear();
            offset = 0;

            if (!String.IsNullOrWhiteSpace(searchText))
            {
                acceptedText = searchText;
                ShowMore(param);
            }
            else
            {
                acceptedText = "";
                ShowMore(param);
            }
        }

        private void CreateGroup(object param)
        {
            int sqlId = 0;
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
                    mainVM.Chats.Insert(0, chat);
                    chat.LastMessage = SystemMessage.ChatroomCreated(DateTime.Now).Message;
                }
                catch (FaultException<ChatroomAlreadyExistExceptionFault> fe)
                {
                    new Views.DialogWindow(fe.Message).ShowDialog();
                    MessageBox.Show(fe.Message);
                }
                catch (Exception ex)
                {
                    new Views.DialogWindow(ex.Message).ShowDialog();
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
                        mainVM.Chats.Insert(0, chat);
                    }
                    catch (Exception ex)
                    {
                        new Views.DialogWindow(ex.Message).ShowDialog();
                    }
                }
                else
                {
                    new Views.DialogWindow("Please enter chatroom name").ShowDialog();
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

        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }

}
