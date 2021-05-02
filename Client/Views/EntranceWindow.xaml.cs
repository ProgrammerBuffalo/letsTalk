namespace Client.Views
{
    /// <summary>
    /// Interaction logic for EntranceWindow.xaml
    /// </summary>
    public partial class EntranceWindow : System.Windows.Window
    {
        public EntranceWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.EntranceViewModel(this);
        }
    }
}
