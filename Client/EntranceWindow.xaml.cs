using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Client
{
    public partial class EntranceWindow : Window, INotifyPropertyChanged
    {
        private bool _isSectionShown = false;

        private ChatService.UploadFileInfo uploadFileInfo;

        public bool IsSectionShown
        {
            get { return _isSectionShown; }
            set { _isSectionShown = value; OnPropertyChanged("IsSectionShown"); }
        }
        public EntranceWindow()
        {
            InitializeComponent();
            DataContext = this.DataContext;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propName));
        }

        private void btn_reg_Click(object sender, RoutedEventArgs e)
        {
            if (IsSectionShown)
            {
                grid_main.IsEnabled = false;
                ChatService.ServerUserInfo registrationInfo = new ChatService.ServerUserInfo()
                {
                    Name = textBox_name.Text,
                    Login = textBox_login.Text,
                    Password = textBox_password.Text,
                };

                if (IsInfoCorrect())
                {
                    MakeRegister(registrationInfo);
                }
            }

            IsSectionShown = true;

        }

        private async void MakeRegister(ChatService.ServerUserInfo registrationInfo)
        {
            animated_control.Visibility = Visibility.Visible;
            animated_control.State = letsTalkControls.Status.Loading;

            var chatClient = new ChatService.ChatClient();
            var fileClient = new ChatService.FileClient();

            try
            {
                int UserId = await chatClient.RegistrationAsync(registrationInfo);

                if (uploadFileInfo.FileStream.CanRead)
                    await fileClient.AvatarUploadAsync(uploadFileInfo.FileExtension, UserId, uploadFileInfo.FileStream);

                animated_control.State = letsTalkControls.Status.Success;
            }
            catch (FaultException<ChatService.LoginExceptionFault> ex)
            {
                animated_control.State = letsTalkControls.Status.Fault;
                textBlock_info.Text = ex.Message;
            }
            catch (FaultException<ChatService.NicknameExceptionFault> ex)
            {
                animated_control.State = letsTalkControls.Status.Fault;
                textBlock_info.Text = ex.Message;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                animated_control.State = letsTalkControls.Status.Fault;
                textBlock_info.Text = "Something wrong with server";
            }

            await Task.Delay(1000);

            animated_control.Visibility = Visibility.Collapsed;
            grid_main.IsEnabled = !grid_main.IsEnabled;

        }

        private bool IsInfoCorrect()
        {
            return true;
        }

        private void btn_photo_Click(object sender, RoutedEventArgs e)
        {
            FileStream fileReader = null;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)" +
                   " | *.jpg; *.jpeg; *.png";

                if (openFileDialog.ShowDialog() == DialogResult.HasValue)
                    return;
                
                uploadFileInfo = new ChatService.UploadFileInfo();

                uploadFileInfo.FileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                uploadFileInfo.FileExtension = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf(".") + 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (fileReader != null)
                    fileReader.Close();
            }
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            IsSectionShown = !IsSectionShown;
            if (uploadFileInfo.FileStream != null) uploadFileInfo.FileStream.Dispose();
        }

        private void btn_sign_Click(object sender, RoutedEventArgs e)
        {
            var chatClient = new ChatService.ChatClient();
            ChatService.ServerUserInfo serverUserInfo = chatClient.Authorization(new ChatService.AuthenticationUserInfo() { Login = textBox_login.Text, Password = textBox_password.Text });

            letsTalkWindow letsTalkWindow = new letsTalkWindow(serverUserInfo.Name, serverUserInfo.SqlId);
            letsTalkWindow.Show();
            this.Close();
        }

    }
}
