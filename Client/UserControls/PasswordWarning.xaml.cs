using System.Windows;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for PasswordWarning.xaml
    /// </summary>
    public partial class PasswordWarning : System.Windows.Controls.UserControl
    {
        private static DependencyProperty TextProperty;

        static PasswordWarning()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PasswordWarning), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }

        public PasswordWarning()
        {
            InitializeComponent();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

    }
}
