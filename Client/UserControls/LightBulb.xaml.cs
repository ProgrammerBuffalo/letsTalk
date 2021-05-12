using System.Windows;
using System.Windows.Controls;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for LightBulb.xaml
    /// </summary>
    public partial class LightBulb : UserControl
    {
        public static readonly DependencyProperty IsOnProperty;
        public static readonly RoutedEvent OnEvent;
        public static readonly RoutedEvent OffEvent;

        static LightBulb()
        {
            IsOnProperty = DependencyProperty.Register("IsOn", typeof(bool), typeof(LightBulb), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsOnChanged));

            OnEvent = EventManager.RegisterRoutedEvent("On", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LightBulb));

            OffEvent = EventManager.RegisterRoutedEvent("Off", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LightBulb));
        }

        public LightBulb()
        {
            InitializeComponent();
        }

        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, value);
        }

        public event RoutedEventHandler On
        {
            add => AddHandler(OnEvent, value);
            remove => RemoveHandler(OnEvent, value);
        }

        public event RoutedEventHandler Off
        {
            add => AddHandler(OnEvent, value);
            remove => AddHandler(OnEvent, value);
        }

        public static void IsOnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            LightBulb bulb = (LightBulb)obj;
            if (bulb.IsOn) bulb.RaiseEvent(new RoutedEventArgs(OnEvent));
            else bulb.RaiseEvent(new RoutedEventArgs(OffEvent));
        }
    }
}
