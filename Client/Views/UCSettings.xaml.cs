namespace Client.Views
{
    /// <summary>
    /// Interaction logic for UCSettings.xaml
    /// </summary>
    public partial class UCSettings : System.Windows.Controls.UserControl
    {
        public UCSettings()
        {
            InitializeComponent();
            DataContext = new ViewModels.SettingsViewModel();
        }
    }
}
