﻿using Client.Utility;
using System;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Client.Models
{
    public class ClientUserInfo : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        //private static ClientUserInfo instance;

        private string userName;
        private string userDesc; // описание пользователя (Hey there i am using Lets Talk!!!)
        private BitmapImage userImage = null; // Аватарка

        //public static ClientUserInfo getInstance()
        //{
        //    return instance;
        //}

        //public ClientUserInfo(string userName, string userDesc, string imagePath, Activity activity)
        //{
        //    UserName = userName;
        //    UserDesc = userDesc;
        //    UserImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + imagePath));
        //    Activity = activity;
        //}

        public ClientUserInfo(int sqlId, string userName)
        {
            SqlId = sqlId;
            UserName = userName;
            //instance = this;
        }

        public int SqlId { private set; get; } // Id в БД

        public string UserName { get => userName; set => Set(ref userName, value); }

        public string UserDesc { get => userDesc; set => Set(ref userDesc, value); }

        public BitmapImage UserImage { get => userImage; set => Set(ref userImage, value); }

        public async void DownloadAvatarAsync()
        {
            ChatService.DownloadRequest request = new ChatService.DownloadRequest(SqlId);
            var avatarClient = new ChatService.AvatarClient();

            Stream stream = null;
            long lenght = 0;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                 {
                     avatarClient.UserAvatarDownload(SqlId, out lenght, out stream);
                     if (lenght <= 0)
                     {
                         Application.Current.Dispatcher.Invoke(() =>
                         {
                             UserImage = new BitmapImage(new Uri("Resources/user.png", UriKind.Relative));
                         });
                         return;
                     }
                      MemoryStream memoryStream = FileHelper.ReadFileByPart(stream);

                     System.Windows.Application.Current.Dispatcher.Invoke(() =>
                     {
                         var bitmapImage = new BitmapImage();
                         bitmapImage.BeginInit();
                         bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                         bitmapImage.StreamSource = memoryStream;
                         bitmapImage.EndInit();

                         UserImage = bitmapImage;
                     });
                     memoryStream.Close();
                     memoryStream.Dispose();
                     stream.Close();
                     stream.Dispose();
                 });

            }
            catch (FaultException<ChatService.ConnectionExceptionFault> ex)
            {
                throw ex;
            }
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop_name));
        }
    }

}
