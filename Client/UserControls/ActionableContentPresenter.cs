namespace Client.UserControls
{
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
