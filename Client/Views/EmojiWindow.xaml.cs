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
            //emojiesGroup.ItemsSource = Emoji.Wpf.EmojiData.AllGroups;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var binding = ((System.Windows.Controls.TextBox)sender).GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding.UpdateSource();
        }

        //private void emojiesGroup_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    emojies.ItemsSource = null;
        //    emojies.Items.Clear();
        //    foreach (var item in Emoji.Wpf.EmojiData.AllGroups)
        //    {
        //        if (item.Icon == ((Emoji.Wpf.EmojiData.Group)emojiesGroup.SelectedItem).Icon)
        //        {
        //            emojies.ItemsSource = item.EmojiList;
        //        }
        //    }
        //}
    }
}
