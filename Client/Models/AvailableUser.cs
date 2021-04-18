namespace Client.Models
{
    public class AvailableUser : System.ComponentModel.INotifyPropertyChanged
    {
        private int sqlId;
        private string name;
        private System.Windows.Media.Imaging.BitmapImage image;
        private bool isOnline;

        public bool IsOnline { get => isOnline; set => Set(ref isOnline, value); }

        public int SqlId { get => sqlId; set => Set(ref sqlId, value); }

        public string Name { get => name; set => Set(ref name, value); }

        public System.Windows.Media.Imaging.BitmapImage Image { get => image; set => Set(ref image, value); }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }
}
