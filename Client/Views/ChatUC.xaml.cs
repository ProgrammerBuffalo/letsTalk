using System.Windows.Controls;
using System.Windows.Media;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for ChatUC.xaml
    /// </summary>
    public partial class ChatUC : UserControl
    {
        public ScrollViewer scroll;

        public ChatUC()
        {
            InitializeComponent();
            Loaded += ChatUC_Loaded;
        }

        private void ChatUC_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var border = (Border)VisualTreeHelper.GetChild(chatListView, 0);
            scroll = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
        }
    }
}
