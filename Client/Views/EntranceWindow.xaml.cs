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
            if (isSignIn) CanLogin();
            else CanRegistr();

            if (error.IsValid)
            {
                if (isNameError)
                {
                    nameWarn.Visibility = System.Windows.Visibility.Hidden;
                    nameText.Width = nameText.ActualWidth + 30;
                    isNameError = false;
                }
            }
            else
            {
                nameWarn.ToolTip = error.ErrorContent;
                if (!isNameError)
                {
                    nameWarn.Visibility = System.Windows.Visibility.Visible;
                    nameText.Width = nameText.ActualWidth - 30;
                    isNameError = true;
                }
            }
        }

        private void loginText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var error = authorizationRules.Validate(loginText.Text, null);
            if (isSignIn) CanLogin();
            else CanRegistr();

            if (error.IsValid)
            {
                if (isLoginError)
                {
                    loginWarn.Visibility = System.Windows.Visibility.Hidden;
                    loginText.Width = loginText.ActualWidth + 30;
                    isLoginError = false;
                }
            }
            else
            {
                loginWarn.ToolTip = error.ErrorContent;
                if (!isLoginError)
                {
                    loginWarn.Visibility = System.Windows.Visibility.Visible;
                    loginText.Width = loginText.ActualWidth - 30;
                    isLoginError = true;
                }
            }
        }

        private void passText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var error = authorizationRules.Validate(passText.Text, null);
            if (isSignIn) CanLogin();
            else CanRegistr();

            if (error.IsValid)
            {
                if (isPassError)
                {
                    passWarn.Visibility = System.Windows.Visibility.Hidden;
                    passText.Width = passText.ActualWidth + 30;
                    isPassError = false;
                }
            }
            else
            {
                passWarn.ToolTip = error.ErrorContent;
                if (!isPassError)
                {
                    passWarn.Visibility = System.Windows.Visibility.Visible;
                    passText.Width = passText.ActualWidth - 30;
                    isPassError = true;
                }
            }
        }

        private void registr_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            nameText.Visibility = System.Windows.Visibility.Visible;
            back.Visibility = System.Windows.Visibility.Visible;
            col2.Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
            signIn.Visibility = System.Windows.Visibility.Hidden;
            Grid.SetColumnSpan(registr, 2);
            Grid.SetColumn(registr, 0);
            nameText.Width = 200;
            loginText.Width = 200;
            passText.Width = 200;
            nameText.Text = "";
            loginText.Text = "";
            passText.Text = "";
            loginWarn.Visibility = System.Windows.Visibility.Hidden;
            passWarn.Visibility = System.Windows.Visibility.Hidden;
            isLoginError = false;
            isPassError = false;
            registr.IsEnabled = false;
            isSignIn = false;
        }

        private void back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            nameText.Visibility = System.Windows.Visibility.Hidden;
            back.Visibility = System.Windows.Visibility.Hidden;
            col2.Width = new System.Windows.GridLength(0, System.Windows.GridUnitType.Pixel);
            signIn.Visibility = System.Windows.Visibility.Visible;
            Grid.SetColumnSpan(registr, 1);
            Grid.SetColumn(registr, 1);
            nameText.Width = 320;
            loginText.Width = 320;
            passText.Width = 320;
            registr.IsEnabled = true;
            nameText.Text = "";
            loginText.Text = "";
            passText.Text = "";
            loginWarn.Visibility = System.Windows.Visibility.Hidden;
            passWarn.Visibility = System.Windows.Visibility.Hidden;
            isLoginError = false;
            isPassError = false;
            isSignIn = true;
        }

        private void CanRegistr()
        {
            if (isNameError || isLoginError || isPassError) registr.IsEnabled = false;
            else registr.IsEnabled = true;
        }

        private void CanLogin()
        {
            if (isLoginError || isPassError) signIn.IsEnabled = false;
            else signIn.IsEnabled = true;
        }
    }
}
