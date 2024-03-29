﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Client.UserControls
{
    /// <summary>
    /// Interaction logic for ChatItem.xaml
    /// </summary>
    public partial class ChatItem : UserControl
    {
        private static readonly DependencyProperty UserNameProperty;
        private static readonly DependencyProperty UserNameFontSizeProperty;
        private static readonly DependencyProperty UserNameForegroundProperty;

        private static readonly DependencyProperty AvatarProperty;
        private static readonly DependencyProperty IsWritingProperty;
        private static readonly DependencyProperty IsOnlineProperty;
        private static readonly DependencyProperty IsOnlineVisibilityProperty;

        private static readonly DependencyProperty DescriptionProperty;
        private static readonly DependencyProperty DescriptionDataTemplateProperty;
        private static readonly DependencyProperty DescriptionDataTemplateSelectorProperty;

        private static readonly RoutedEvent WritingOnEvent;
        private static readonly RoutedEvent WritingOffEvent;

        static ChatItem()
        {
            UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            UserNameFontSizeProperty = DependencyProperty.Register("UserNameFontSize", typeof(float), typeof(ChatItem), new FrameworkPropertyMetadata(14f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            UserNameForegroundProperty = DependencyProperty.Register("UserNameForeground", typeof(System.Windows.Media.Brush), typeof(ChatItem), new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)); ;

            AvatarProperty = DependencyProperty.Register("Avatar", typeof(BitmapImage), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            IsWritingProperty = DependencyProperty.Register("IsWriting", typeof(string), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, WritingChanged));

            IsOnlineProperty = DependencyProperty.Register("IsOnline", typeof(bool), typeof(ChatItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            IsOnlineVisibilityProperty = DependencyProperty.Register("IsOnlineVisibility", typeof(Visibility), typeof(ChatItem), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            DescriptionProperty = DependencyProperty.Register("Description", typeof(object), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            DescriptionDataTemplateProperty = DependencyProperty.Register("DescriptionDataTemplate", typeof(DataTemplate), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            DescriptionDataTemplateSelectorProperty = DependencyProperty.Register("DescriptionDataTemplateSelector", typeof(DataTemplateSelector), typeof(ChatItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            WritingOnEvent = EventManager.RegisterRoutedEvent("WritingOn", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChatItem));

            WritingOffEvent = EventManager.RegisterRoutedEvent("WritingOff", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChatItem));
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

        public float UserNameFontSize
        {
            get => (float)GetValue(UserNameFontSizeProperty);
            set => SetValue(UserNameFontSizeProperty, value);
        }

        public System.Windows.Media.Brush UserNameForeground
        {
            get => (System.Windows.Media.Brush)GetValue(UserNameForegroundProperty);
            set => SetValue(UserNameForegroundProperty, value);
        }

        public BitmapImage Avatar
        {
            get => (BitmapImage)GetValue(AvatarProperty);
            set => SetValue(AvatarProperty, value);
        }

        public string IsWriting
        {
            get => (string)GetValue(IsWritingProperty);
            set => SetValue(IsWritingProperty, value);
        }

        public bool IsOnline
        {
            get => (bool)GetValue(IsOnlineProperty);
            set => SetValue(IsOnlineProperty, value);
        }

        public Visibility IsOnlineVisibility
        {
            get => (Visibility)GetValue(IsOnlineVisibilityProperty);
            set => SetValue(IsOnlineVisibilityProperty, value);
        }

        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public DataTemplate DescriptionDataTemplate
        {
            get => (DataTemplate)GetValue(DescriptionDataTemplateProperty);
            set => SetValue(DescriptionDataTemplateProperty, value);
        }

        public DataTemplateSelector DescriptionDataTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(DescriptionDataTemplateSelectorProperty);
            set => SetValue(DescriptionDataTemplateSelectorProperty, value);
        }

        public event RoutedEventHandler WritingOn
        {
            add => AddHandler(WritingOnEvent, value);
            remove => RemoveHandler(WritingOnEvent, value);
        }

        public event RoutedEventHandler WritingOff
        {
            add => AddHandler(WritingOffEvent, value);
            remove => RemoveHandler(WritingOffEvent, value);
        }

        private static void WritingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ChatItem item = (ChatItem)obj;
            if (item.IsWriting == null) item.RaiseEvent(new RoutedEventArgs(WritingOffEvent));
            else item.RaiseEvent(new RoutedEventArgs(WritingOnEvent));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }

}
