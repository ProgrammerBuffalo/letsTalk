using Client.Models;
using Client.Utility;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Client.ViewModels
{
    class AddUserViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<AvailableUser> allUsers;

        private int offset;
        private int count;

        public AddUserViewModel()
        {
            ShowMoreCommand = new Command(ShowMore);

            AllUsers = new ObservableCollection<AvailableUser>();

            offset = 10;
            count = 0;
        }

        public ICommand ShowMoreCommand { get; }
        public ICommand SearchChangedCommand { get; }

        public ObservableCollection<AvailableUser> AllUsers { get => allUsers; set => Set(ref allUsers, value); }

        public void ShowMore(object param)
        {

        }

        public void SearchChanged(object param)
        {
            string name = param.ToString();
            if (!String.IsNullOrWhiteSpace(name))
            {
                AllUsers.Clear();
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
        public CreateGroupViewModel()
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
