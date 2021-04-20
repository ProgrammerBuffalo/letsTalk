using Client.Utility;
using Client.Views;
using Client.UserControls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModels
{
    class EntranceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string fileName;
        private System.Windows.Window entranceWindow;

        private bool isSectionShown;

        private string name;
        private string login;
        private string password;
        private string info;

        private bool formIsEnabled;

        private System.Windows.Visibility loaderVisbility;
        private LoaderState loaderState;

        private ChatService.UploadFileInfo uploadFileInfo;

        public EntranceViewModel(System.Windows.Window window)
        {
            entranceWindow = window;

            IsSectionShown = false;
            LoaderVisibility = System.Windows.Visibility.Collapsed;

            SignInCommand = new Command(SignIn);
            RegistrateCommand = new Command(Registrate);
            SetPhotoCommand = new Command(SetPhoto);
            BackCommand = new Command(Back);
        }

        public ICommand SignInCommand { get; }
        public ICommand RegistrateCommand { get; }
        public ICommand SetPhotoCommand { get; }
        public ICommand BackCommand { get; }

        public bool IsSectionShown { get => isSectionShown; set => Set(ref isSectionShown, value); }

        public string Name { get => name; set => Set(ref name, value); }
        public string Login { get => login; set => Set(ref login, value); }
        public string Password { get => password; set => Set(ref password, value); }
        public string Info { get => info; set => Set(ref info, value); }

        // бывший main grid visibility
        public bool FormIsEnabled { get => formIsEnabled; set => Set(ref formIsEnabled, value); }

        // статус и видимость супер крутой загрузки
        public LoaderState LoaderState { get => loaderState; set => Set(ref loaderState, value); }
        public System.Windows.Visibility LoaderVisibility { get => loaderVisbility; set => Set(ref loaderVisbility, value); }

        private void SignIn(object param)
        {
            var unitClient = new ChatService.UnitClient();
            try
            {
                ChatService.ServerUserInfo serverUserInfo = unitClient.Authorization(new ChatService.AuthenticationUserInfo() { Login = Login, Password = Password });

                MainWindow mainWindow = new MainWindow();
                mainWindow.DataContext = new MainViewModel(serverUserInfo.Name, serverUserInfo.SqlId);
                mainWindow.Show();
                entranceWindow.Close();
            }
            catch (FaultException<ChatService.AuthorizationExceptionFault> ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                Info = ex.Message;
            }
        }

        private async void Registrate(object param)
        {
            if (IsSectionShown)
            {
                FormIsEnabled = false;
                ChatService.ServerUserInfo registrationInfo = new ChatService.ServerUserInfo()
                {
                    Name = Name,
                    Login = Login,
                    Password = Password
                };
                if (IsInfoCorrect())
                {
                    LoaderVisibility = System.Windows.Visibility.Visible;
                    LoaderState = LoaderState.Loading;

                    MakeRegister(registrationInfo);
                    LoaderState = LoaderState.Success;

                    await Task.Delay(1000); // нужен для того чтобы анимация закончилась до конца

                    LoaderVisibility = System.Windows.Visibility.Collapsed;
                    FormIsEnabled = !FormIsEnabled;
                }
            }
            IsSectionShown = true;
        }


        private async void MakeRegister(ChatService.ServerUserInfo registrationInfo)
        {

            var unitClient = new ChatService.UnitClient(); // Работает с net.tcp (регистрация)
            var avatarClient = new ChatService.AvatarClient(); // Работает с http (отправка аватарки)

            try
            {
                int UserId = await unitClient.RegistrationAsync(registrationInfo);

                if (uploadFileInfo != null)
                {

                    uploadFileInfo.FileName = fileName;
                    uploadFileInfo.FileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read); ;

                    if (uploadFileInfo.FileStream.CanRead)
                        await avatarClient.AvatarUploadAsync(uploadFileInfo.FileName, UserId, uploadFileInfo.FileStream);
                }
            }
            catch (FaultException<ChatService.LoginExceptionFault> ex)
            {
                LoaderState = LoaderState.Fault;
                Info = ex.Message;
            }
            catch (FaultException<ChatService.NicknameExceptionFault> ex)
            {
                LoaderState = LoaderState.Fault;
                Info = ex.Message;
            }
            catch (FaultException<ChatService.StreamExceptionFault> ex)
            {
                LoaderState = LoaderState.Fault;
                Info = ex.Message;
            }
            catch (FaultException<ChatService.AuthorizationExceptionFault> ex)
            {
                LoaderState = LoaderState.Fault;
                Info = ex.Message;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                LoaderState = LoaderState.Fault;
                Info = "Something wrong with server";
            }
            finally
            {
                if (uploadFileInfo != null)
                {
                    if (uploadFileInfo.FileStream != null)
                        uploadFileInfo.FileStream.Dispose();
                }
            }

        }

        private void SetPhoto(object param)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg; *.jpeg; *.png";

                if (openFileDialog.ShowDialog() == true)
                    uploadFileInfo = new ChatService.UploadFileInfo();

                fileName = openFileDialog.FileName;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void Back(object param)
        {
            IsSectionShown = !IsSectionShown;
            if (uploadFileInfo != null && uploadFileInfo.FileStream != null) uploadFileInfo.FileStream.Dispose();
        }

        private bool IsInfoCorrect()
        {
            return true;
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
