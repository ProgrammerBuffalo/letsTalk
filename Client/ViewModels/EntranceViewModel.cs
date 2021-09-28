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

        private string info;
        private bool formIsEnabled;

        private BitmapImage image;
        private MemoryStream memoryStream;

        private string name;
        private string login;
        private string password;

        private System.Windows.Visibility loaderVisbility;
        private LoaderState loaderState;

        private ChatService.UploadFileInfo uploadFileInfo;

        public EntranceViewModel(System.Windows.Window window)
        {
            entranceWindow = window;

            LoaderVisibility = System.Windows.Visibility.Collapsed;

            SignInCommand = new Command(SignIn);
            RegistrateCommand = new Command(Registrate);
            SetPhotoCommand = new Command(SetPhoto);
            BackCommand = new Command(Back);
            CancelImageCommand = new Command(CancelImage);

            string language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Models.Settings.Instance.ChangeLanguage(language);
        }

        public ICommand SignInCommand { get; }
        public ICommand RegistrateCommand { get; }
        public ICommand SetPhotoCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CancelImageCommand { get; }

        public string Info { get => info; set => Set(ref info, value); }

        public BitmapImage SelectedImage { get => image; set => Set(ref image, value); }

        public string Name { get => name; set => Set(ref name, value); }
        public string Login { get => login; set => Set(ref login, value); }
        public string Password { get => password; set => Set(ref password, value); }

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
                    Models.Settings.Instance.AddUser(serverUserInfo.SqlId);
                    MainWindow window = new MainWindow();
                    MainViewModel viewModel = new MainViewModel(serverUserInfo.Name, serverUserInfo.SqlId);
                    viewModel.AddUC += window.AddUC;
                    viewModel.RemoveUC += window.RemoveUC;
                    window.DataContext = viewModel;
                    window.Show();
                    entranceWindow.Close();
                }
                catch (FaultException<ChatService.ConnectionExceptionFault> ex)
                {
                    new DialogWindow(App.Current.Resources["ConnectionError"].ToString()).ShowDialog();
                    Info = ex.Message;
                }
                catch (Exception ex)
                {
                    new DialogWindow(ex.Message).ShowDialog();
                }
            }
            else
            {
                new DialogWindow(App.Current.Resources["AllFieldsError"].ToString()).ShowDialog();
            }
        }

        private async void Registrate(object param)
        {
            if (formIsEnabled)
            {
                ChatService.ServerUserInfo registrationInfo = new ChatService.ServerUserInfo()
                {
                    Name = Name,
                    Login = Login,
                    Password = Password
                };

                LoaderVisibility = System.Windows.Visibility.Visible;
                LoaderState = LoaderState.Loading;

                MakeRegister(registrationInfo);
                LoaderState = LoaderState.Success;

                await Task.Delay(1000); // нужен для того чтобы анимация закончилась до конца

                LoaderVisibility = System.Windows.Visibility.Collapsed;
            }
            formIsEnabled = true;
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
                    memoryStream.Position = 0;
                    uploadFileInfo.FileName = fileName;

                    uploadFileInfo.FileStream = memoryStream;

                    if (uploadFileInfo.FileStream.CanRead)
                        await avatarClient.UserAvatarUploadAsync(uploadFileInfo.FileName, UserId, uploadFileInfo.FileStream);
                }
            }
            catch (FaultException<ChatService.LoginExceptionFault>)
            {
                LoaderState = LoaderState.Fault;
                new DialogWindow(App.Current.Resources["LoginError"].ToString()).ShowDialog();
            }
            catch (FaultException<ChatService.NicknameExceptionFault>)
            {
                LoaderState = LoaderState.Fault;
                new DialogWindow(App.Current.Resources["PasswordError"].ToString()).ShowDialog();
            }
            catch (FaultException<ChatService.StreamExceptionFault>)
            {
                LoaderState = LoaderState.Fault;
                new DialogWindow(App.Current.Resources["StreamError"].ToString()).ShowDialog();
            }
            catch (FaultException<ChatService.AuthorizationExceptionFault>)
            {
                LoaderState = LoaderState.Fault;
                new DialogWindow(App.Current.Resources["AuthorizationError"].ToString()).ShowDialog();
            }
            catch (Exception)
            {
                LoaderState = LoaderState.Fault;
                new DialogWindow(App.Current.Resources["ServerError"].ToString()).ShowDialog();
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
                    memoryStream = new MemoryStream();
                    Bitmap croppedSource = null;
                    if (source.Height > 1000 || source.Width > 1000)
                        croppedSource = source.Clone(new Rectangle(250, 250, 750, 750), source.PixelFormat);
                    else
                        croppedSource = source.Clone(new Rectangle(0, 0, source.Width, source.Height), source.PixelFormat);

                    switch (fileName.Substring(fileName.LastIndexOf(".")))
                    {
                        case ".jpg": croppedSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg); break;
                        case ".png": croppedSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png); break;
                    }

                    memoryStream.Position = 0;

                    var bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = memoryStream;
                    bitmap.EndInit();

                    SelectedImage = bitmap;
                }
                else
                {
                    SelectedImage = null;
                    memoryStream = null;
                    uploadFileInfo = null;
                }

            }
            catch (Exception)
            {
                new DialogWindow(App.Current.Resources["StreamError"].ToString()).ShowDialog();
            }
        }

        private void Back(object param)
        {
            formIsEnabled = false;
            if (uploadFileInfo != null && uploadFileInfo.FileStream != null)
                uploadFileInfo.FileStream.Dispose();
        }

        private void CancelImage(object param)
        {
            SelectedImage = null;
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
