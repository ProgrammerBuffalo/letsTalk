
namespace Client.UserControls
{
    //public class ActionableContentPresenter : ContentPresenter
    //{
    //    public event DependencyPropertyChangedEventHandler ContentChanged;
    //    public event DependencyPropertyChangedEventHandler ContentTemplateChanged;

    //    static ActionableContentPresenter()
    //    {
    //        ContentProperty.OverrideMetadata(typeof(ActionableContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentChanged)));

    //        ContentTemplateProperty.OverrideMetadata(typeof(ActionableContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentTempalteChanged)));
    //    }

    //    private static void OnContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    //    {
    //        var control = (ActionableContentPresenter)obj;
    //        control?.ContentChanged?.Invoke(control, new DependencyPropertyChangedEventArgs(ContentProperty, e.OldValue, e.NewValue));
    //    }

    //    private static void OnContentTempalteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    //    {
    //        var control = (ActionableContentPresenter)obj;
    //        control.ContentTemplateChanged?.Invoke(control, new DependencyPropertyChangedEventArgs(ContentTemplateProperty, e.OldValue, e.NewValue));
    //    }

    //    protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
    //    {
    //        base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);
    //    }
    //}

    public class ActionableContentPresenter : System.Windows.Controls.ContentPresenter
    {
        public static System.Windows.RoutedEvent ContentChangedEvent;

        static ActionableContentPresenter()
        {
            ContentChangedEvent = System.Windows.EventManager.RegisterRoutedEvent("ContentChanged", System.Windows.RoutingStrategy.Bubble, typeof(System.Windows.RoutedEventHandler), typeof(ActionableContentPresenter));
        }

        public event System.Windows.RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

        public static void AddContentChangedHandler(System.Windows.UIElement el, System.Windows.RoutedEventHandler handler)
        {
            el.AddHandler(ContentChangedEvent, handler);
        }

        public static void RemoveContentChangedHandler(System.Windows.UIElement el, System.Windows.RoutedEventHandler handler)
        {
            el.RemoveHandler(ContentChangedEvent, handler);
        }

        protected override void OnVisualChildrenChanged(System.Windows.DependencyObject visualAdded, System.Windows.DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            RaiseEvent(new System.Windows.RoutedEventArgs(ContentChangedEvent, this));
        }
    }
}
