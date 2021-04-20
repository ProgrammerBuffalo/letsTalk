using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for UC2.xaml
    /// </summary>
    public partial class UC2 : UserControl
    {
        public UC2()
        {
            InitializeComponent();
            DataContext = new ViewModels.AddUserViewModel();
        }
    }
}
