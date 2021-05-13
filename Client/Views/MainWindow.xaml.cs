﻿namespace Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            hamburgerMenu.HamburgerButtonClick += HamburgerMenu_HamburgerButtonClick;
            hamburgerMenu.Loaded += HamburgerMenu_Loaded;
        }

        public void AddUC(System.Windows.Controls.UserControl uc)
        {
            System.Windows.Controls.Grid.SetColumn(uc, 1);
            grid.Children.Add(uc);
        }

        public void RemoveUC(System.Windows.Controls.UserControl uc)
        {
            grid.Children.Remove(uc);
        }

        private void HamburgerMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (hamburgerMenu.IsPaneOpen)
            {
                col1.Width = new System.Windows.GridLength(hamburgerMenu.Width);
                hamburgerMenu.OpenPaneLength = hamburgerMenu.Width;
            }
            else
            {
                col1.Width = new System.Windows.GridLength(hamburgerMenu.CompactPaneLength);
            }
        }

        private void HamburgerMenu_HamburgerButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!hamburgerMenu.IsPaneOpen)
            {
                col1.Width = new System.Windows.GridLength(hamburgerMenu.Width);
                hamburgerMenu.OpenPaneLength = hamburgerMenu.Width;
            }
            else
            {
                col1.Width = new System.Windows.GridLength(hamburgerMenu.CompactPaneLength);
            }
        }

        //private void DoubleAnimation_Completed(object sender, System.EventArgs e)
        //{
        //    col1.Width = new System.Windows.GridLength(hamburgerMenu.Width);
        //    hamburgerMenu.OpenPaneLength = hamburgerMenu.Width;
        //}
    }
}
