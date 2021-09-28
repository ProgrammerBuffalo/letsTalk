namespace Client.Views
{
    /// <summary>
    /// Interaction logic for Temp.xaml
    /// </summary>
    public partial class Temp : System.Windows.Controls.UserControl
    {
        public Temp()
        {
            InitializeComponent();
        }

        public void AddUc(System.Windows.UIElement ui)
        {
            System.Windows.Controls.Grid.SetColumn(ui, 1);
            grid.Children.Add(ui);
        }

        public void RemoveUC(System.Windows.UIElement ui)
        {
            grid.Children.Remove(ui);
        }
    }
}
