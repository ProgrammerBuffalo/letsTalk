using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for ChatItem.xaml
    /// </summary>
    public partial class ChatItem : UserControl
    {
        static readonly DependencyProperty UserNameProperty;
        static readonly DependencyProperty DescriptionProperty;
        static readonly DependencyProperty ActivityProperty;
        static readonly DependencyProperty CountProperty;
        static readonly DependencyProperty CountVisibilityProperty;
        static readonly DependencyProperty PathProperty;
        static readonly DependencyProperty AvatarProperty;
        static readonly DependencyProperty ActivityVisibilityProperty;

        static ChatItem()
        {
            UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            ActivityProperty = DependencyProperty.Register("Activity", typeof(Models.Activity), typeof(ChatItem), new FrameworkPropertyMetadata(Models.Activity.Offline, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            CountProperty = DependencyProperty.Register("Count", typeof(short), typeof(ChatItem), new FrameworkPropertyMetadata((short)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            CountVisibilityProperty = DependencyProperty.Register("CountVisibility", typeof(Visibility), typeof(ChatItem), new FrameworkPropertyMetadata(Visibility.Hidden, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            AvatarProperty = DependencyProperty.Register("Avatar", typeof(BitmapImage), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            ActivityVisibilityProperty = DependencyProperty.Register("ActivityVisibility", typeof(Visibility), typeof(ChatItem), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)); ; ;
        }

        public ChatItem() : base()
        {
            InitializeComponent();
        }

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public Models.Activity Activity
        {
            get => (Models.Activity)GetValue(ActivityProperty);
            set => SetValue(ActivityProperty, value);
        }

        public short Count
        {
            get => (short)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public Visibility CountVisibility
        {
            get => (Visibility)GetValue(CountVisibilityProperty);
            set => SetValue(CountVisibilityProperty, value);
        }

        public BitmapImage Avatar
        {
            get => (BitmapImage)GetValue(AvatarProperty);
            set => SetValue(AvatarProperty, value);
        }

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public Visibility ActivityVisibility
        {
            get => (Visibility)GetValue(ActivityVisibilityProperty);
            set => SetValue(ActivityVisibilityProperty, value);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }

}
