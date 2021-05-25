using System.Windows;
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
        public delegate void GetControlDelegate(ref ScrollViewer element);
        public GetControlDelegate getControl;

        public ChatUC()
        {
            InitializeComponent();
            Loaded += ChatUC_Loaded;
        }

        private void ChatUC_Loaded(object sender, RoutedEventArgs e)
        {
            var border = (Border)VisualTreeHelper.GetChild(chatListView, 0);
            scroll = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            getControl.Invoke(ref scroll);
            CanNotifyText();
        }

        private void canNotifyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CanNotifyText();
        }

        private void CanNotifyText()
        {
            if (canNotifyCheckBox.IsChecked.HasValue)
            {
                if (canNotifyCheckBox.IsChecked.Value)
                    canNotifyText.Text = "rington is on";
                else if (!canNotifyCheckBox.IsChecked.Value)
                    canNotifyText.Text = "rington is off";
            }
            else canNotifyText.Text = "global settings";
        }
    }
}
