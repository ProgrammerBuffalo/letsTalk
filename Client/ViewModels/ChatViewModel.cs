using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Client.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private delegate void MessageSendType(string message);
        private MessageSendType sendType;

        private double _previousScrollOffset;

        //public MainViewModel.ChatDelegate RemoveChat { get; set; }
        private MainViewModel mainVM;

        public ChatService.ChatClient ChatClient { get; set; }

        private ClientUserInfo client;
        //private Chat chat;

        private MediaMessage curMediaMessage;
        private MediaPlayer player;

        private Timer timer;

        private string isWritingText;
        private string messageText = "";

        private Visibility loaderVisibility;

        public ChatViewModel(ChatService.ChatClient chatClient, MainViewModel mainVM)
        {
            this.mainVM = mainVM;

            client = ClientUserInfo.getInstance();
            ChatClient = chatClient;
            //Settings = Settings.Instance;

            TextBox_KeyDownCommand = new Command(TextBox_KeyDown);
            TextBox_KeyUpCommand = new Command(TextBox_KeyUp);
            TextBox_EnterPressedCommand = new Command(TextBox_EnterPressed);

            MediaPlayCommand = new Command(MediaPlay);
            MediaPosChangedCommand = new Command(MediaPosChanged);
            SendCommand = new Command(Send);
            OpenFileCommand = new Command(OpenFile);
            UnloadCommand = new Command(Unload);
            LoadCommand = new Command(Load);

            //AddUserCommand = new Command(AddMember);
            //RemoveUserCommand = new Command(RemoveMember);
            EditChatCommand = new Command(EditChat);
            LeaveChatCommand = new Command(LeaveChat);
            //DeleteChatCommand = new Command(DeleteChat);

            DownloadFileCommand = new Command(DownloadFile);

            sendType = SendText;

            player = new MediaPlayer();
            player.MediaEnded += MediaEnded;

            timer = new Timer();
            timer.Elapsed += MediaPosTimer;
            timer.Interval = 500;

            LoaderVisibility = Visibility.Hidden;
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

        public ICommand EditChatCommand { get; }
        //public ICommand AddUserCommand { get; }
        //public ICommand RemoveUserCommand { get; }
        public ICommand LeaveChatCommand { get; }

        //public ICommand DeleteChatCommand { get; }

        public ICommand LoadCommand { get; }
        public ICommand DownloadFileCommand { get; }

        public Chat Chat { get => mainVM.SelectedChat; }
        //public Chat Chat { get => chat; set => Set(ref chat, value); }
        public string IsWritingText { get => isWritingText; set => Set(ref isWritingText, value); }
        public string MessageText { get => messageText; set => Set(ref messageText, value); }
        public System.Windows.Controls.ScrollViewer Scroll { get; set; }
        public Settings Settings { get; }

        public Visibility LoaderVisibility { get => loaderVisibility; set => Set(ref loaderVisibility, value); }

        public void SetScrollViewer(ref System.Windows.Controls.ScrollViewer scroll)
        {
            Scroll = scroll;
            Scroll.ScrollChanged += ScrollPositionChanged;
            Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
        }

        private async void ScrollPositionChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            //если чем выше значение тем раньше произойдет загрузка
            if (Scroll.VerticalOffset == 0 && _previousScrollOffset != Scroll.VerticalOffset)
            {
                _previousScrollOffset = Scroll.VerticalOffset;
                LoaderVisibility = Visibility.Visible;
                Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
                await LoadMore();
                Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                LoaderVisibility = Visibility.Hidden;
            }
            else
                _previousScrollOffset = Scroll.VerticalOffset;
        }

        private async void Load(object obj)
        {
            await LoadMore();
            Scroll.ScrollToEnd();
        }

        private async Task LoadMore()
        {
            ChatService.UnitClient unitClient = new ChatService.UnitClient();
            ChatService.ServiceMessage[] serviceMessages = await unitClient.MessagesFromOneChatAsync(mainVM.SelectedChat.SqlId, mainVM.ClientUserInfo.SqlId, mainVM.SelectedChat._messageOffset, mainVM.SelectedChat._messageCount, mainVM.SelectedChat._offsetDate);

            if (serviceMessages == null)
            {
                mainVM.SelectedChat.Messages.Add(SystemMessage.ShiftDate(mainVM.SelectedChat._offsetDate));
                mainVM.SelectedChat._offsetDate.AddDays(-1);
                return;
            }

            if (serviceMessages.First().DateTime == DateTime.MinValue)
                return;

            if (serviceMessages != null)
            {
                ObservableCollection<Models.SourceMessage> messages =
                new ObservableCollection<Models.SourceMessage>(await System.Threading.Tasks.Task<List<SourceMessage>>.Run(() =>
                {
                    List<SourceMessage> messagesFromChat = new List<SourceMessage>();
                    foreach (var message in serviceMessages)
                    {
                        if (message is ChatService.ServiceMessageText)
                        {
                            var textMessage = message as ChatService.ServiceMessageText;
                            messagesFromChat.Add(mainVM.SelectedChat.GetMessageType(textMessage.UserId, new TextMessage(textMessage.Text, textMessage.DateTime)));
                        }
                        else if(message is ChatService.ServiceMessageFile)
                        {
                            var fileMessage = message as ChatService.ServiceMessageFile;
                            messagesFromChat.Add(mainVM.SelectedChat.GetMessageType(fileMessage.UserId, new FileMessage(fileMessage.FileName, fileMessage.DateTime, fileMessage.StreamId) { IsLoaded = true }));
                        }
                        else
                        {
                            var systemMessage = message as ChatService.ServiceMessageManage;
                            switch (systemMessage.RulingMessage)
                            {
                                case ChatService.RulingMessage.UserJoined:
                                    messagesFromChat.Add(SystemMessage.UserAdded(systemMessage.DateTime, systemMessage.UserNickname));
                                    break;
                                case ChatService.RulingMessage.UserLeft:
                                    messagesFromChat.Add(SystemMessage.UserLeftChat(systemMessage.DateTime, systemMessage.UserNickname));
                                    break;
                                case ChatService.RulingMessage.UserRemoved:
                                    messagesFromChat.Add(SystemMessage.UserRemoved(systemMessage.DateTime, systemMessage.UserNickname));
                                    break;
                            }
                        }

                    }

                    return messagesFromChat;
                }));

                foreach (var message in messages)
                    mainVM.SelectedChat.Messages.Insert(0, message);

                mainVM.SelectedChat._messageOffset += mainVM.SelectedChat._messageCount;
            }
        }

        private void TextBox_EnterPressed(object obj)
        {
            if (MessageText.Length < 1)
                return;
            ChatClient.SendMessageTextAsync(new ChatService.ServiceMessageText() { Text = MessageText, UserId = client.SqlId }, mainVM.SelectedChat.SqlId);
            mainVM.SelectedChat.Messages.Add(mainVM.SelectedChat.GetMessageType(client.SqlId, new TextMessage(MessageText, DateTime.Now)));
            MessageText = "";
        }

        private void TextBox_KeyUp(object obj)
        {
            if (MessageText.Length < 1)
                ChatClient.MessageIsWritingAsync(mainVM.SelectedChat.SqlId, null);
        }

        private void TextBox_KeyDown(object obj)
        {
            if (MessageText.Length > 1)
                ChatClient.MessageIsWritingAsync(mainVM.SelectedChat.SqlId, client.SqlId);
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
        //public void AddMember(object param)
        //{
        //    (chat as ChatGroup).AddMember(AvaibleUser user);
        //}

        //тут должен быть метод для сообшения другим пользователям что узера удалили
        //public void RemoveMember(object param)
        //{
        //    (chat as ChatGroup).RemoveMember();
        //}

        private void EditChat(object param)
        {
            Views.EditGroupWindow window = new Views.EditGroupWindow();
            window.DataContext = new EditGroupViewModel(mainVM, ChatClient);
            window.ShowDialog();
        }

        //тут должен быть твой метод для сообшения другим пользователям что узер покинул chat
        public void LeaveChat(object param)
        {
            //chat.Messages.Add(SystemMessage.UserLeavedChat(client.UserName));
        }

        //тут должен быть твой метод для сообшения другим пользователям что узер покинул chat
        //public void DeleteChat(object param)
        //{
        //    RemoveChat.Invoke(Chat);
        //}

        //тут метод для загрузки файла
        public async void DownloadFile(object param)
        {

            FileMessage message = (FileMessage)param;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string extension = message.FileName.Substring(message.FileName.LastIndexOf('.'));
            saveFileDialog.Filter = $"(*{extension}*)|*{extension}*";
            saveFileDialog.FileName = message.FileName;
            if (saveFileDialog.ShowDialog() != true)
                return;

            string filename = saveFileDialog.FileName;
            if (message is ImageMessage)
            {
                BitmapImage imageMessage = (message as ImageMessage).Bitmap;

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(imageMessage));

                using (var filestream = new FileStream(filename, System.IO.FileMode.Create))
                {
                    encoder.Save(filestream);
                }
            }

            Stream stream = null;
            MemoryStream memoryStream = null;
            FileStream fileStream = null;
            long lenght = 0;

            ChatService.FileClient fileClient = new ChatService.FileClient();
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    string name = fileClient.FileDownload(message.StreamId, out lenght, out stream);
                    if (lenght <= 0)
                        return;
                    memoryStream = FileHelper.ReadFileByPart(stream);

                    fileStream = new FileStream(filename, FileMode.Create);
                    memoryStream.CopyTo(fileStream);
                });
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
            finally
            {
                if (fileStream != null) fileStream.Close();
                if (memoryStream != null) memoryStream.Close();
                if (stream != null) stream.Close();
            }
        }

        private void MediaPlay(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (player.Source == null)
            {
                curMediaMessage = message;
                player.Open(new Uri(message.FileName, UriKind.Absolute));
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
                player.Open(new Uri(message.FileName, UriKind.Absolute));
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

        private void ShowMore(object param)
        {
            LoadMore();
        }

        private void SendText(string text)
        {

        }

        private void SendFile(string path)
        {
            string extn = path.Substring(path.LastIndexOf('.'));
            if (extn == ".mp3" || extn == ".wave")
            {
                mainVM.SelectedChat.Messages.Add(mainVM.SelectedChat.GetMessageType(client.SqlId, new MediaMessage(path, DateTime.Now)));
                return;
            }

            ChatService.FileClient fileClient = new ChatService.FileClient();

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Guid stream_id = Guid.Empty;
            try
            {
                if (fileStream.CanRead)
                    stream_id = fileClient.FileUpload(mainVM.SelectedChat.SqlId, path, client.SqlId, fileStream);
            }
            catch (Exception ex) { }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
            mainVM.SelectedChat.Messages.Add(new SessionSendedMessage(new FileMessage(path, DateTime.Now, stream_id)));
        }

        private void MediaPosChanged(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (message == curMediaMessage)
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
        }

        private void Unload(object param)
        {
            ChatClient.MessageIsWriting(mainVM.SelectedChat.SqlId, null);
            if (curMediaMessage != null)
            {
                curMediaMessage.IsPlaying = false;
                curMediaMessage.CurrentLength = 0;
                curMediaMessage = null;
                player.Close();
            }
            mainVM.SelectedChat.Messages.Clear();

        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}