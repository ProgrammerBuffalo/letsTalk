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
        public delegate void GetControlDelegate(DependencyObject element);
        public GetControlDelegate getScroll;
        public GetControlDelegate getRichTextBox;

        public ChatUC()
        {
            InitializeComponent();
            contentLoader.Visibility = Visibility.Visible;
            Loaded += ChatUC_Loaded;
        }

        private void ChatUC_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var border = (Border)VisualTreeHelper.GetChild(chatListView, 0);
                scroll = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                getScroll.Invoke(scroll);
            }
            catch (System.Exception ex) { MessageBox.Show(ex.Message); }
            CanNotifyText();
            contentLoader.Visibility = Visibility.Hidden;
        }

        private void canNotifyCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CanNotifyText();
        }

        private void CanNotifyText()
        {
            if (canNotifyCheckBox.IsChecked.HasValue)
            {
                if (canNotifyCheckBox.IsChecked.Value) canNotifyText.Text = "rington is on";
                else if (!canNotifyCheckBox.IsChecked.Value) canNotifyText.Text = "rington is off";
            }
            else canNotifyText.Text = "global settings";
        }

        private void input_ContentChanged(object sender, RoutedEventArgs e)
        {
            var rich = VisualTreeHelper.GetChild(input, 0);
            if (rich is RichTextBox) getRichTextBox.Invoke(rich);
        }
    }
}
