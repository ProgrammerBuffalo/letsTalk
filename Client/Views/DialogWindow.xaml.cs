namespace Client.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : System.Windows.Window
    {
        public DialogWindow(string message)
        {
            InitializeComponent();
            this.message.Text = message;
        }

        public DialogWindow(string title, string message) : this(message)
        {
            Title = title;
        }
    }
}
