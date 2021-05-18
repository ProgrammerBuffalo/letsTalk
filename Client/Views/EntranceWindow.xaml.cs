namespace Client.Views
{
    /// <summary>
    /// Interaction logic for EntranceWindow.xaml
    /// </summary>
    public partial class EntranceWindow : MahApps.Metro.Controls.MetroWindow
    {
        public EntranceWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.EntranceViewModel(this);
        }
    }
}
