using Client.Models;
using Client.Utility;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private ObservableCollection<AvailableUser> users;
        private ObservableCollection<AvailableUser> allUsers;

        private bool canDrop;

        public EditGroupViewModel(MainViewModel mainVM)
        {
            this.mainVM = mainVM;

            Chat = (ChatGroup)mainVM.SelectedChat;

            RemoveMemberCommand = new Command(RemoveMember);
            AddMemberCommand = new Command(AddMember);
            DeleteChatCommand = new Command(DeleteChat);
            ChangeImageCommand = new Command(ChangeImage);
            ShowMoreCommand = new Command(ShowMore);

            Users_MouseLeaveCommand = new Command(Users_MouseLeave);
            Users_PreviewDragEnterCommand = new Command(Users_PreviewDragEnter);
            Users_DragLeaveCommand = new Command(Users_DragLeave);
        }

        public ICommand RemoveMemberCommand { get; }
        public ICommand AddMemberCommand { get; }
        public ICommand DeleteChatCommand { get; }
        public ICommand ChangeImageCommand { get; }
        public ICommand ShowMoreCommand { get; }

        public ICommand Users_MouseLeaveCommand { get; }
        public ICommand Users_PreviewDragEnterCommand { get; }
        public ICommand Users_DragLeaveCommand { get; }

        public ObservableCollection<AvailableUser> Users { get => users; set => Set(ref users, value); }
        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }
        public ChatGroup Chat { get => chat; set => Set(ref chat, value); }

        private void RemoveMember(object param)
        {
            //AvailableUser user = (AvailableUser)param;
            //mainVM.SelectedChat.RemoveUser(user);
        }

        private void AddMember(object param)
        {
            //Chat.Users.Add(user)
        }

        private void DeleteChat(object param)
        {
            //mainVM.Chats.Remove(mainVM.SelectedChat);
        }

        private void ChangeImage(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                //тут метод сохранения нового фото на сервере
                BitmapImage image = new BitmapImage(new Uri(dialog.FileName));
                chat.Image = image;
            }
        }

        private void ShowMore(object param)
        {
            //foreach (var user in users)
            //allUsers.Add(user);
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
                Users.Add(sourceItem);
                AllUsers.Remove(sourceItem);
            }
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
