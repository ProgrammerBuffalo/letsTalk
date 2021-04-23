using System.Windows;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for LoginWarning.xaml
    /// </summary>
    public partial class LoginWarning : System.Windows.Controls.UserControl
    {
        static DependencyProperty TextProperty;

        static LoginWarning()
        {
            DependencyProperty.Register("Text", typeof(string), typeof(LoginWarning), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }

        public LoginWarning()
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
