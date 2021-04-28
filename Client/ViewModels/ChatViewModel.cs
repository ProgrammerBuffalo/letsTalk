﻿using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private delegate void MessageSendType(string message);
        private MessageSendType sendType;
        public MainViewModel.ChatDelegate RemoveChat { get; set; }

        public ChatService.ChatClient ChatClient { get; set; }

        private ClientUserInfo client;
        private Chat chat;
        private Settings settings;

        private MediaMessage curMediaMessage;
        private MediaPlayer player;

        private Timer timer;

        private string isWritingText;
        private string messageText = "";

        private Visibility loaderVisibility;

        public ChatViewModel(ChatService.ChatClient chatClient)
        {
            client = ClientUserInfo.getInstance();
            ChatClient = chatClient;
            settings = Settings.Instance;
            Scroll.ScrollChanged += ScrollScrollChanged;

            TextBox_KeyDownCommand = new Command(TextBox_KeyDown);
            TextBox_KeyUpCommand = new Command(TextBox_KeyUp);
            TextBox_EnterPressedCommand = new Command(TextBox_EnterPressed);

            MediaPlayCommand = new Command(MediaPlay);
            MediaPosChangedCommand = new Command(MediaPosChanged);
            SendCommand = new Command(Send);
            OpenSmileCommand = new Command(OpenSmile);
            OpenFileCommand = new Command(OpenFile);
            UnloadCommand = new Command(Unload);

            AddUserCommand = new Command(AddMember);
            RemoveUserCommand = new Command(RemoveMember);
            LeaveChatCommand = new Command(LeaveChat);
            DeleteChatCommand = new Command(DeleteChat);

            DownloadFileCommand = new Command(DownloadFile);

            sendType = SendText;

            player = new MediaPlayer();
            player.MediaEnded += MediaEnded;

            timer = new Timer();
            timer.Elapsed += MediaPosTimer;
            timer.Interval = 500;
        }

        public ChatViewModel(Chat chat, ChatService.ChatClient chatClient) : this(chatClient)
        {
            Chat = chat;
        }

        public ICommand TextBox_KeyDownCommand { get; }
        public ICommand TextBox_KeyUpCommand { get; }
        public ICommand TextBox_EnterPressedCommand { get; }

        public ICommand MediaPlayCommand { get; }
        public ICommand MediaPosChangedCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenSmileCommand { get; }
        public ICommand UnloadCommand { get; }

        public ICommand AddUserCommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand LeaveChatCommand { get; }
        public ICommand DeleteChatCommand { get; }

        public ICommand DownloadFileCommand { get; }

        public Chat Chat { get => chat; set => Set(ref chat, value); }
        public string IsWritingText { get => isWritingText; set => Set(ref isWritingText, value); }
        public string MessageText { get => messageText; set => Set(ref messageText, value); }
        public System.Windows.Controls.ScrollViewer Scroll { get; set; }

        public Visibility LoaderVisibility { get => loaderVisibility; set => Set(ref loaderVisibility, value); }

        private void ScrollScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (Scroll.VerticalOffset == Scroll.ScrollableHeight)
            {
                LoaderVisibility = Visibility.Visible;
                //тут твой метод для загрузки доп сообшений (в параметр идет эта переменная settings.MessageLoadCount)

                //в конце твоего асинхроного методв 
                App.Current.Dispatcher.Invoke(() => { loaderVisibility = Visibility.Hidden; });
            }
        }

        private void TextBox_EnterPressed(object obj)
        {
            if (MessageText.Length < 1)
                return;
            //ChatClient.SendMessageTextAsync();
        }

        private void TextBox_KeyUp(object obj)
        {
            if (MessageText.Length < 1)
                ChatClient.MessageIsWritingAsync(Chat.SqlId, null);
        }

        private void TextBox_KeyDown(object obj)
        {
            if (MessageText.Length > 1)
                ChatClient.MessageIsWritingAsync(chat.SqlId, client.SqlId);
            System.Windows.MessageBox.Show(MessageText);
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            player.Close();
            timer.Stop();
            curMediaMessage.CurrentLength = 0;
            curMediaMessage.IsPlaying = false;
        }

        private void MediaPosTimer(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                curMediaMessage.CurrentLength = player.Position.Ticks;
            });
        }

        //тут должен быть твой метод для сообшения другим пользователям что добавлен новый узер
        public void AddMember(object param)
        {
            //(chat as ChatGroup).AddMember(AvaibleUser user);
        }

        //тут должен быть метод для сообшения другим пользователям что узера удалили
        public void RemoveMember(object param)
        {
            //(chat as ChatGroup).RemoveMember();
        }

        //тут должен быть твой метод для сообшения другим пользователям что узер покинул chat
        public void LeaveChat(object param)
        {
            //chat.Messages.Add(SystemMessage.UserLeavedChat(client.UserName));
        }

        //тут должен быть твой метод для сообшения другим пользователям что узер покинул chat
        public void DeleteChat(object param)
        {
            //RemoveChat.Invoke(Chat);
        }

        //тут метод для загрузки файла
        public void DownloadFile(object param)
        {
            //FileMessage message = (FileMessage)((SourceMessage)param).Message;
            //message.IsLoaded = true;
        }

        private void MediaPlay(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (player.Source == null)
            {
                curMediaMessage = message;
                player.Open(new Uri(message.Path, UriKind.Absolute));
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
                player.Play();
                timer.Start();
                curMediaMessage.IsPlaying = true;
            }
            else if (message != curMediaMessage)
            {
                curMediaMessage.IsPlaying = false;
                curMediaMessage.CurrentLength = 0;
                player.Close();
                player.Open(new Uri(message.Path, UriKind.Absolute));
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
                player.Play();
                timer.Start();
                curMediaMessage = message;
                curMediaMessage.IsPlaying = true;
            }
            else if (curMediaMessage.IsPlaying)
            {
                player.Pause();
                timer.Stop();
                curMediaMessage.IsPlaying = false;
            }
            else
            {
                player.Play();
                timer.Start();
                curMediaMessage.IsPlaying = true;
            }
        }

        private void Send(object param)
        {
            sendType.Invoke(IsWritingText);
        }

        private void OpenFile(object param)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files (*.txt, *.docx, *.doc)|*.txt;*.docx;*.doc"
                 + "|Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
                 + "|Presentation files (*.pptx)|*.pptx"
                 + "|Audio files (*.mp3, *.vawe)|*.mp3;*.vawe"
                 + "|Zip files (*.zip)|*.zip";
            if (dialog.ShowDialog() == true)
            {
                sendType = SendFile;
                IsWritingText = dialog.FileName;
            }
        }

        private void OpenSmile(object param)
        {

        }

        private void SendText(string text)
        {

        }

        private void SendFile(string path)
        {
            sendType = SendText;
        }

        private void MediaPosChanged(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (message == curMediaMessage)
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
        }

        private void Unload(object param)
        {
            if (curMediaMessage != null)
            {
                curMediaMessage.IsPlaying = false;
                curMediaMessage.CurrentLength = 0;
                curMediaMessage = null;
                player.Close();
            }
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}

