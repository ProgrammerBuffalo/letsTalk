namespace Client.Views
{
    /// <summary>
    /// Interaction logic for UserControlView.xaml
    /// </summary>
    public partial class UCAvailableUsers : System.Windows.Controls.UserControl
    {
        public UCAvailableUsers()
        {
            InitializeComponent();
            DataContext = new ViewModels.AvailableUsersViewModel();

        }
    }
}
