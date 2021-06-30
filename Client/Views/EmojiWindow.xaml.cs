namespace Client.Views
{
    /// <summary>
    /// Interaction logic for EmojiWindow.xaml
    /// </summary>
    public partial class EmojiWindow : System.Windows.Window
    {
        public EmojiWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
