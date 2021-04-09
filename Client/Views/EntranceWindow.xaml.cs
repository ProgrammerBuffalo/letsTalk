namespace Client.Views
{
    /// <summary>
    /// Interaction logic for EntranceWindow.xaml
    /// </summary>
    public partial class EntranceWindow : System.Windows.Window
    {
        public EntranceWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.EntranceViewModel(this);
        }

        //private bool _isSectionShown = false; // Нужен для переключения окна между авторизацией и регистрацией

        //private ChatService.UploadFileInfo uploadFileInfo; // Нужен для отправки аватарки серверу

        //public bool IsSectionShown
        //{
        //    get { return _isSectionShown; }
        //    set { _isSectionShown = value; OnPropertyChanged("IsSectionShown"); }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;

        //public void OnPropertyChanged(string propName)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //        handler(this, new PropertyChangedEventArgs(propName));
        //}

        //// Регистрация пользователя
        //private void btn_reg_Click(object sender, RoutedEventArgs e)
        //{
        //    if (IsSectionShown)
        //    {
        //        grid_main.IsEnabled = false;
        //        ChatService.ServerUserInfo registrationInfo = new ChatService.ServerUserInfo()
        //        {
        //            Name = textBox_name.Text,
        //            Login = textBox_login.Text,
        //            Password = textBox_password.Text,
        //        };

        //        if (IsInfoCorrect())
        //        {
        //            MakeRegister(registrationInfo);
        //        }
        //    }

        //    IsSectionShown = true;

        //}

        //private async void MakeRegister(ChatService.ServerUserInfo registrationInfo)
        //{
        //    animated_control.Visibility = Visibility.Visible;
        //    animated_control.State = UserControls.LoadingUserControl.Status.Loading;

        //    var chatClient = new ChatService.ChatClient(); // Работает с net.tcp (регистрация)
        //    var fileClient = new ChatService.FileClient(); // Работает с http (отправка аватарки)

        //    try
        //    {
        //        int UserId = await chatClient.RegistrationAsync(registrationInfo);

        //        if (uploadFileInfo.FileStream.CanRead)
        //            await fileClient.AvatarUploadAsync(uploadFileInfo.FileExtension, UserId, uploadFileInfo.FileStream);

        //        animated_control.State = UserControls.LoadingUserControl.Status.Success;
        //    }
        //    catch (FaultException<ChatService.LoginExceptionFault> ex)
        //    {
        //        animated_control.State = UserControls.LoadingUserControl.Status.Fault;
        //        textBlock_info.Text = ex.Message;
        //    }
        //    catch (FaultException<ChatService.NicknameExceptionFault> ex)
        //    {
        //        animated_control.State = UserControls.LoadingUserControl.Status.Fault;
        //        textBlock_info.Text = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        animated_control.State = UserControls.LoadingUserControl.Status.Fault;
        //        textBlock_info.Text = "Something wrong with server";
        //    }

        //    await Task.Delay(1000); // нужен для того чтобы анимация закончилась до конца

        //    animated_control.Visibility = Visibility.Collapsed;
        //    grid_main.IsEnabled = !grid_main.IsEnabled;

        //}

        //private bool IsInfoCorrect()
        //{
        //    return true;
        //}

        //// Указываем какую фотографию мы отправим серверу, но пока не отправляем
        //private void btn_photo_Click(object sender, RoutedEventArgs e)
        //{
        //    FileStream fileReader = null;

        //    try
        //    {
        //        OpenFileDialog openFileDialog = new OpenFileDialog();
        //        openFileDialog.Multiselect = false;
        //        openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)" +
        //           " | *.jpg; *.jpeg; *.png";

        //        if (openFileDialog.ShowDialog() == DialogResult.HasValue)
        //            return;

        //        uploadFileInfo = new ChatService.UploadFileInfo();

        //        uploadFileInfo.FileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
        //        uploadFileInfo.FileExtension = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf(".") + 1);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        if (fileReader != null)
        //            fileReader.Close();
        //    }
        //}

        //private void btn_back_Click(object sender, RoutedEventArgs e)
        //{
        //    IsSectionShown = !IsSectionShown;
        //    if (uploadFileInfo.FileStream != null) uploadFileInfo.FileStream.Dispose();
        //}

        //// Если авторизация произошла успешно, то открывается окно LetsTalkWindow (Главное окно)
        //private void btn_sign_Click(object sender, RoutedEventArgs e)
        //{
        //    var chatClient = new ChatService.ChatClient();
        //    ChatService.ServerUserInfo serverUserInfo = chatClient.Authorization(new ChatService.AuthenticationUserInfo() { Login = textBox_login.Text, Password = textBox_password.Text });

        //    LetsTalkWindow letsTalkWindow = new LetsTalkWindow(serverUserInfo.Name, serverUserInfo.SqlId);
        //    letsTalkWindow.Show();
        //    this.Close();
        //}
    }
}
