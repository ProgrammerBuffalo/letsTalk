using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{

    public partial class letsTalkWindow : Window
    {
        //private ObservableCollection<MenuItem> OptionsMenu = new ObservableCollection<MenuItem>();

        //private readonly NavigationService navigationService;

        public MainViewModel viewModel { get; set; }

        public letsTalkWindow()
        {
            InitializeComponent();

            //this.navigationService = new NavigationService();
            //this.navigationService.Navigated += this.NavigationServiceEx_OnNavigated;
            //this.hamburgerMenu.Content = this.navigationService.Frame;

            //OptionsMenu.Add(new MenuItem()
            //{
            //    Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.AccountPlus,
            //                                    HorizontalAlignment = HorizontalAlignment.Center,
            //                                    VerticalAlignment = VerticalAlignment.Center,
            //                                    Width=22, Height=22 },
            //    Label = "Add User",
            //    NavigationType = typeof(UsersPage),
            //    NavigationDestination = new Uri("Pages/UserControlView.xaml", UriKind.RelativeOrAbsolute)
            //});

            //OptionsMenu.Add(new MenuItem()
            //{
            //    Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.AccountAlert,
            //                                    HorizontalAlignment = HorizontalAlignment.Center,
            //                                    VerticalAlignment = VerticalAlignment.Center,
            //                                    Width = 22, Height = 22 },
            //    Label = "Settings",
            //    NavigationType = typeof(GreenPage),
            //    NavigationDestination = new Uri("Pages/GreenPage.xaml", UriKind.RelativeOrAbsolute)

            //});

            //hamburgerMenu.OptionsItemsSource = this.OptionsMenu;
        }

        public letsTalkWindow(string name, int sqlId) : this()
        {
            viewModel = new MainViewModel(name, sqlId);

            this.DataContext = viewModel;
        }

        private void hamburgerMenu_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
           // if (e.InvokedItem is MenuItem menuItem && menuItem.IsNavigation)
          //  {
           //     this.navigationService.Navigate(menuItem.NavigationDestination);
          //  }
        }

        private void NavigationServiceEx_OnNavigated(object sender, NavigationEventArgs e)
        {         

            //this.hamburgerMenu.SelectedOptionsItem = this.hamburgerMenu
            //                                         .OptionsItems
            //                                         .OfType<MenuItem>()
            //                                         .FirstOrDefault(x => x.NavigationDestination == e.Uri);
        }

    }
}
