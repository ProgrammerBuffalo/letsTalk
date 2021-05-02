namespace Client.Utility
{
    public delegate void UCChangedEventHandler(System.Windows.Controls.UserControl uc);

    interface IHelperUC
    {
        event UCChangedEventHandler RemoveUC;
        event UCChangedEventHandler AddUC;
    }
}
