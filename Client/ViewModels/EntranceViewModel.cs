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
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Client.ViewModels
{
    class EntranceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string fileName;
        private System.Windows.Window entranceWindow;

        private bool isSectionShown;

        private string info;
        private bool formIsEnabled;

        private BitmapImage image;
        private MemoryStream memoryStream;

        private string name;
        private string login;
        private string password;

        private bool nameIsWarning;
        private bool loginIsWarning;
        private bool passwordIsWarning;

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
            CancelImageCommand = new Command(CancelImage);
        }

        public ICommand SignInCommand { get; }
        public ICommand RegistrateCommand { get; }
        public ICommand SetPhotoCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CancelImageCommand { get; }

        public bool IsSectionShown { get => isSectionShown; set => Set(ref isSectionShown, value); }

        public string Info { get => info; set => Set(ref info, value); }

        public BitmapImage SelectedImage { get => image; set => Set(ref image, value); }

        public string Name { get => name; set => Set(ref name, value); }
        public string Login { get => login; set => Set(ref login, value); }
        public string Password { get => password; set => Set(ref password, value); }

        public bool NameIsWarning { get => nameIsWarning; set => Set(ref nameIsWarning, value); }
        public bool LoginIsWarning { get => loginIsWarning; set => Set(ref loginIsWarning, value); }
        public bool PasswordIsWarning { get => passwordIsWarning; set => Set(ref passwordIsWarning, value); }

        // бывший main grid visibility
        public bool FormIsEnabled { get => formIsEnabled; set => Set(ref formIsEnabled, value); }

        public LoaderState LoaderState { get => loaderState; set => Set(ref loaderState, value); }
        public System.Windows.Visibility LoaderVisibility { get => loaderVisbility; set => Set(ref loaderVisbility, value); }

        private void SignIn(object param)
        {
            if (login != null && password != null)
            {
                var unitClient = new ChatService.UnitClient();
                try
                {
                    ChatService.ServerUserInfo serverUserInfo = unitClient.Authorization(new ChatService.AuthenticationUserInfo() { Login = Login, Password = Password });

                    MainWindow window = new MainWindow();
                    MainViewModel viewModel = new MainViewModel(serverUserInfo.Name, serverUserInfo.SqlId);
                    viewModel.AddUC += window.AddUC;
                    viewModel.RemoveUC += window.RemoveUC;
                    window.DataContext = viewModel;
                    window.Show();
                    entranceWindow.Close();
                }
                catch (FaultException<ChatService.AuthorizationExceptionFault> ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    Info = ex.Message;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("all fields must be entered");
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
                    this.memoryStream.Position = 0;
                    uploadFileInfo.FileName = fileName;
                
                    uploadFileInfo.FileStream = memoryStream;

                    if (uploadFileInfo.FileStream.CanRead)
                        await avatarClient.UserAvatarUploadAsync(uploadFileInfo.FileName, UserId, uploadFileInfo.FileStream);
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
                {
                    fileName = openFileDialog.FileName;
                    uploadFileInfo = new ChatService.UploadFileInfo();

                    Bitmap source = new Bitmap(fileName);
                    this.memoryStream = new MemoryStream();
                    Bitmap croppedSource = null;
                    if (source.Height > 1000 || source.Width > 1000)
                    {
                        croppedSource = source.Clone(new Rectangle(250, 250, 750, 750), source.PixelFormat);
                    }
                    else
                    {
                        croppedSource = source.Clone(new Rectangle(0, 0, source.Width, source.Height), source.PixelFormat);
                    }

                    switch (fileName.Substring(fileName.LastIndexOf(".")))
                    {
                        case ".jpg": croppedSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg); break;
                        case ".png": croppedSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png); break;
                    }

                    this.memoryStream.Position = 0;

                    var bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    this.SelectedImage = bitmap;
                }
                else
                {
                    this.SelectedImage = null;
                    memoryStream = null;
                    uploadFileInfo = null;
                }

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

        private void CancelImage(object param)
        {
            SelectedImage = null;
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
