using System.Windows;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for PasswordWarning.xaml
    /// </summary>
    public partial class PasswordWarning : System.Windows.Controls.UserControl
    {
        private static readonly DependencyProperty TextProperty;
        private static readonly DependencyProperty IsWarningProperty;
        private static readonly DependencyProperty ErrorTextProperty;

        static PasswordWarning()
        {
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PasswordWarning), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            IsWarningProperty = DependencyProperty.Register("IsWarning", typeof(bool), typeof(PasswordWarning), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IsWarningChanged)));

            ErrorTextProperty = DependencyProperty.Register("ErrorText", typeof(string), typeof(PasswordWarning), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
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

        public bool IsWarning
        {
            get => (bool)GetValue(IsWarningProperty);
            set => SetValue(IsWarningProperty, value);
        }

        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        private static void IsWarningChanged(DependencyObject @object, DependencyPropertyChangedEventArgs args)
        {
            PasswordWarning uc = (PasswordWarning)@object;
            if (uc.IsWarning)
            {
                uc.grid.ColumnDefinitions[1].Width = new GridLength(uc.Height);
            }
            else
            {
                uc.grid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }
    }
}
