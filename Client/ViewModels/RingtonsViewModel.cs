using Client.Models;
using Client.Utility;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Client.ViewModels
{
    class RingtonsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected Settings settings;
        protected Rington selectedRington;
        public RingtonsViewModel(Settings settings)
        {
            this.settings = settings;
            Ringtons = new ObservableCollection<Rington>();
        }

        public ICommand RingtonChangedCommand { get; protected set; }

        public ObservableCollection<Rington> Ringtons { get; protected set; }
        public Rington SelectedRington { get => selectedRington; set => Set(ref selectedRington, value); }


        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }

    }

    class UserRingtonsViewModel : RingtonsViewModel
    {
        public UserRingtonsViewModel(Settings settings) : base(settings)
        {
            RingtonChangedCommand = new Command(UserRingtonChanged);

            var ringtons = settings.GetRingtons();
            foreach (var item in ringtons)
            {
                Ringtons.Add(item);
                if (item.Name == settings.SelectedUserRington.Name) item.IsSelected = true;
            }
        }

        private void UserRingtonChanged(object param)
        {
            settings.SelectedUserRington = (Rington)param;
            settings.PlayRington(settings.SelectedUserRington.Path);
        }
    }

    class GroupRingtonsViewModel : RingtonsViewModel
    {
        public GroupRingtonsViewModel(Settings settings) : base(settings)
        {
            RingtonChangedCommand = new Command(GroupRingtonChanged);

            var ringtons = settings.GetRingtons();
            foreach (var item in ringtons)
            {
                Ringtons.Add(item);
                if (item.Name == settings.SelectedGroupRington.Name) item.IsSelected = true;
            }
        }

        private void GroupRingtonChanged(object param)
        {
            settings.SelectedGroupRington = (Rington)param;
            settings.PlayRington(settings.SelectedGroupRington.Path);
        }
    }
}
