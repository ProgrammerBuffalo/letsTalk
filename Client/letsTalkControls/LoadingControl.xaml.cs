using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Client.letsTalkControls
{
    

    public partial class LoadingControl : UserControl
    {
        public static readonly DependencyProperty stateProperty;

        public LoadingControl()
        {
            InitializeComponent();
        }

        static LoadingControl()
        {
            stateProperty = DependencyProperty.Register("State", typeof(Status), typeof(LoadingControl));
        }

        public Status State
        {
            get { return (Status)GetValue(stateProperty); }
            set { SetValue(stateProperty, value); }
        }

    }

    public enum Status
    {
        Loading = 0,
        Success = 1,
        Fault = 2
    }
}
