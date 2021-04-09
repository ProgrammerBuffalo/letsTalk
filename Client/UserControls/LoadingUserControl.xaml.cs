using System.Windows;
using System.Windows.Controls;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for LoadingUserControl.xaml
    /// </summary>
    public partial class LoadingUserControl : UserControl
    {
        static LoadingUserControl()
        {
            stateProperty = DependencyProperty.Register("State", typeof(LoaderState), typeof(LoadingUserControl), new FrameworkPropertyMetadata(LoaderState.Loading, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }
        
        public static readonly DependencyProperty stateProperty; 

        public LoadingUserControl()
        {
            InitializeComponent();
        }

        public LoaderState State
        {
            get { return (LoaderState)GetValue(stateProperty); }
            set { SetValue(stateProperty, value); }
        }        
    }

    public enum LoaderState
    {
        Loading = 0,
        Success = 1,
        Fault = 2
    }
}
