namespace Client.Views
{
    /// <summary>
    /// Interaction logic for AddUser.xaml
    /// </summary>
    public partial class AddUser : System.Windows.Controls.UserControl
    {
        public AddUser()
        {
            InitializeComponent();
        }

        //надо убрать
        public AddUser(ChatService.ChatClient chatClient)
        {
            InitializeComponent();
            DataContext = new ViewModels.AddUserViewModel(chatClient);
        }
    }
}
