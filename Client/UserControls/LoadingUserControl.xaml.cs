using System.Windows;
using System.Windows.Controls;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for LoadingUserControl.xaml
    /// </summary>
    public partial class LoadingUserControl : UserControl
    {
        public static readonly DependencyProperty StateProperty; 
        
        static LoadingUserControl()
        {
            StateProperty = DependencyProperty.Register("State", typeof(LoaderState), typeof(LoadingUserControl), new FrameworkPropertyMetadata(LoaderState.Loading, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        }

        public LoadingUserControl()
        {
            InitializeComponent();
        }

        public LoaderState State
        {
            get { return (LoaderState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }        
    }

    public enum LoaderState
    {
        Loading = 0,
        Success = 1,
        Fault = 2
    }
}
