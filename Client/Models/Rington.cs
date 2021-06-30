namespace Client.Models
{
    public class Rington : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private bool isSelected;

        public Rington()
        {

        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
