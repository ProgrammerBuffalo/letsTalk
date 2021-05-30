using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for EntranceWindow.xaml
    /// </summary>
    public partial class EntranceWindow : MahApps.Metro.Controls.MetroWindow
    {
        private static Utility.AuthorizationRules authorizationRules;
        private static Utility.NameRules nameRules;
        private bool isSignIn;
        private bool isNameError;
        private bool isLoginError;
        private bool isPassError;

        public EntranceWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.EntranceViewModel(this);
            authorizationRules = new Utility.AuthorizationRules();
            nameRules = new Utility.NameRules();
            isSignIn = true;
            signIn.IsEnabled = false;
        }

        private void nameText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var error = nameRules.Validate(nameText.Text, null);
            if (error.IsValid)
            {
                nameWarn.Visibility = System.Windows.Visibility.Hidden;
                col1.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
                isNameError = false;
                CanRegistr();
            }
            else
            {
                nameWarn.ToolTip = error.ErrorContent;
                nameWarn.Visibility = System.Windows.Visibility.Visible;
                col1.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
                isNameError = true;
                if (isSignIn) signIn.IsEnabled = false;
                else registr.IsEnabled = false;
            }
        }

        private void loginText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var error = authorizationRules.Validate(loginText.Text, null);
            if (error.IsValid)
            {
                loginWarn.Visibility = System.Windows.Visibility.Hidden;
                col2.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
                isLoginError = false;
                if (isSignIn) CanLogin();
                else CanRegistr();

            }
            else
            {
                loginWarn.ToolTip = error.ErrorContent;
                loginWarn.Visibility = System.Windows.Visibility.Visible;
                col2.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
                isLoginError = true;
                if (isSignIn) signIn.IsEnabled = false;
                else registr.IsEnabled = false;
            }
        }

        private void passText_TextChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            ((ViewModels.EntranceViewModel)this.DataContext).Password = ((PasswordBox)sender).Password;
            var error = authorizationRules.Validate(passText.Password, null);
            if (error.IsValid)
            {
                passWarn.Visibility = System.Windows.Visibility.Hidden;
                col3.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
                isPassError = false;
                if (isSignIn) CanLogin();
                else CanRegistr();
            }
            else
            {
                passWarn.ToolTip = error.ErrorContent;
                passWarn.Visibility = System.Windows.Visibility.Visible;
                col3.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
                isPassError = true;
                if (isSignIn) signIn.IsEnabled = false;
                else registr.IsEnabled = false;
            }
        }

        private void registr_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            nameText.Visibility = System.Windows.Visibility.Visible;
            back.Visibility = System.Windows.Visibility.Visible;
            mainCol2.Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            signIn.Visibility = System.Windows.Visibility.Hidden;
            Grid.SetColumnSpan(registr, 2);
            Grid.SetColumn(registr, 0);

            if (isNameError)
            {
                col1.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
                nameWarn.Visibility = System.Windows.Visibility.Visible;
            }
            else col1.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);

            if (isLoginError) col2.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
            else col2.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);

            if (isPassError) col3.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
            else col3.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
            isSignIn = false;
            registr.IsEnabled = false;
        }

        private void back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            nameText.Visibility = System.Windows.Visibility.Hidden;
            back.Visibility = System.Windows.Visibility.Hidden;
            mainCol2.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
            signIn.Visibility = System.Windows.Visibility.Visible;
            Grid.SetColumnSpan(registr, 1);
            Grid.SetColumn(registr, 1);

            if (isLoginError) col2.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
            else col2.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);

            if (isPassError) col3.Width = new System.Windows.GridLength(40, System.Windows.GridUnitType.Pixel);
            else col3.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
            nameWarn.Visibility = System.Windows.Visibility.Hidden;
            registr.IsEnabled = true;
            isSignIn = true;
            nameText.Text = "";
            nameWarn.Visibility = System.Windows.Visibility.Hidden;
            CanLogin();
        }

        private void CanRegistr()
        {
            if (isNameError || isLoginError || isPassError ||
                string.IsNullOrWhiteSpace(nameText.Text) ||
                string.IsNullOrWhiteSpace(loginText.Text) ||
                string.IsNullOrWhiteSpace(passText.Password)) registr.IsEnabled = false;
            else registr.IsEnabled = true;
        }

        private void CanLogin()
        {
            if (isLoginError || isPassError ||
                string.IsNullOrWhiteSpace(loginText.Text) ||
                string.IsNullOrWhiteSpace(passText.Password)) signIn.IsEnabled = false;
            else signIn.IsEnabled = true;
        }

    }
}
