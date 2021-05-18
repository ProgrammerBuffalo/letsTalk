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
        //private delegate void MessageSendType(string message);
        //private MessageSendType sendType;

        //private bool canWrite = true;
        public event PropertyChangedEventHandler PropertyChanged;

        private int _countLeft;

        private double _previousScrollOffset;

        private MainViewModel mainVM;
        private Chat chat;

        private MediaMessage curMediaMessage;
        private MediaPlayer player;

        private Timer timer;

        private string isWritingText;
        private string messageText = "";

        private Visibility loaderVisibility;
        private Message inputMessage;

        public ChatViewModel(MainViewModel mainVM)
        {
            this.mainVM = mainVM;
            Chat = mainVM.SelectedChat;
            Chat.ClientId = mainVM.Client.SqlId;
            Settings = Settings.Instance;

            InputMessage = new TextMessage();
            LoaderVisibility = Visibility.Hidden;

            _countLeft = chat._messageCount;

            //client = ClientUserInfo.getInstance();
            //ChatClient = chatClient;
            //sendType = SendText;

            player = new MediaPlayer();
            player.MediaEnded += MediaEnded;

            timer = new Timer();
            timer.Elapsed += MediaPosTimer;
            timer.Interval = 500;

            TextBox_KeyDownCommand = new Command(TextBox_KeyDown);
            TextBox_KeyUpCommand = new Command(TextBox_KeyUp);
            TextBox_EnterPressedCommand = new Command(TextBox_EnterPressed);

            MediaPlayCommand = new Command(MediaPlay);
            MediaPosChangedCommand = new Command(MediaPosChanged);
            //SendCommand = new Command(Send);
            OpenFileCommand = new Command(OpenFile);
            UnloadCommand = new Command(Unload);
            LoadCommand = new Command(Load);

            EditChatCommand = new Command(EditChat);
            LeaveChatCommand = new Command(LeaveChat);

            DownloadFileCommand = new Command(DownloadFile);
            CancelFileCommand = new Command(CancelFile);
            SendFileCommand = new Command(SendFile);

            InputMessage = new TextMessage();
        }

        public ICommand TextBox_KeyDownCommand { get; }
        public ICommand TextBox_KeyUpCommand { get; }
        public ICommand TextBox_EnterPressedCommand { get; }

        public ICommand MediaPlayCommand { get; }
        public ICommand MediaPosChangedCommand { get; }
        //public ICommand SendCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenSmileCommand { get; }
        public ICommand UnloadCommand { get; }

        public ICommand EditChatCommand { get; }
        public ICommand LeaveChatCommand { get; }

        public ICommand LoadCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand CancelFileCommand { get; }
        public ICommand DownloadFileCommand { get; }

        public Chat Chat { get => chat; set => Set(ref chat, value); }
        public Settings Settings { get; }
        public string IsWritingText { get => isWritingText; set => Set(ref isWritingText, value); }

        public Message InputMessage { get => inputMessage; set => Set(ref inputMessage, value); }
        public string MessageText { get => messageText; set => Set(ref messageText, value); }
        public System.Windows.Controls.ScrollViewer Scroll { get; set; }

        public Visibility LoaderVisibility { get => loaderVisibility; set => Set(ref loaderVisibility, value); }

        public void SetScrollViewer(ref System.Windows.Controls.ScrollViewer scroll)
        {
            Scroll = scroll;
            Scroll.ScrollChanged += ScrollPositionChanged;
            Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
        }

        private async void ScrollPositionChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (Scroll.VerticalOffset == 0 && _previousScrollOffset != Scroll.VerticalOffset)
            {
                _countLeft = chat._messageCount;
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
            ChatService.ServiceMessage[] serviceMessages = await unitClient.MessagesFromOneChatAsync(Chat.SqlId, mainVM.Client.SqlId, Chat._messageOffset, Chat._messageCount, Chat._offsetDate);

            if (serviceMessages == null)
            {
                Chat.Messages.Insert(0, SystemMessage.ShiftDate(Chat._offsetDate));
                Chat._messageOffset = 0;
                Chat._offsetDate = Chat._offsetDate.AddDays(-1);
                if (_countLeft > 0)
                    await LoadMore();
                return;
            }

            if (serviceMessages.First().DateTime == DateTime.MinValue)
            {
                return;
            }

            if (serviceMessages.First().DateTime == DateTime.MaxValue)
            {
                Chat.Messages.Insert(0, SystemMessage.ShiftDate(Chat._offsetDate));
                Chat._messageOffset = 0;
                Chat._offsetDate = Chat._offsetDate.AddDays(-1);
                await LoadMore();
                return;
            }

            if (serviceMessages != null)
            {
                ObservableCollection<SourceMessage> messages =
                new ObservableCollection<SourceMessage>(await System.Threading.Tasks.Task.Run(() =>
                {
                    List<SourceMessage> messagesFromChat = new List<SourceMessage>();
                    foreach (var message in serviceMessages)
                    {
                        if (message is ChatService.ServiceMessageText)
                        {
                            var textMessage = message as ChatService.ServiceMessageText;
                            messagesFromChat.Add(Chat.GetMessageType(mainVM.Client.SqlId, textMessage.UserId, new TextMessage(textMessage.Text, textMessage.DateTime)));
                        }
                        else if (message is ChatService.ServiceMessageFile)
                        {
                            var fileMessage = message as ChatService.ServiceMessageFile;
                            messagesFromChat.Add(Chat.GetMessageType(mainVM.Client.SqlId, fileMessage.UserId, new FileMessage(fileMessage.FileName, fileMessage.DateTime, fileMessage.StreamId) { IsLoaded = true }));
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

                Chat._messageOffset += messages.Count;
                _countLeft -= messages.Count;

                foreach (var message in messages)
                    Chat.Messages.Insert(0, message);

                if (_countLeft > 0)
                {
                    await LoadMore();
                    return;
                }

            }
        }

        private void TextBox_EnterPressed(object obj)
        {
            if (String.IsNullOrEmpty(messageText)) return;
            mainVM.ChatClient.SendMessageTextAsync(new ChatService.ServiceMessageText() { Text = MessageText, UserId = mainVM.Client.SqlId }, Chat.SqlId);
            Chat.Messages.Add(new UserMessage(new TextMessage(messageText, DateTime.Now)));
            MessageText = null;
            Scroll.ScrollToBottom();
        }

        private void TextBox_KeyUp(object obj)
        {
            if (String.IsNullOrEmpty(messageText))
                mainVM.ChatClient.MessageIsWritingAsync(Chat.SqlId, null);
        }

        private void TextBox_KeyDown(object obj)
        {
            if (String.IsNullOrEmpty(messageText))
                mainVM.ChatClient.MessageIsWritingAsync(Chat.SqlId, mainVM.Client.SqlId);
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

        private void EditChat(object param)
        {
            Views.EditGroupWindow window = new Views.EditGroupWindow();
            window.DataContext = new EditGroupViewModel(mainVM);
            window.ShowDialog();
        }

        public void LeaveChat(object param)
        {
            if (Chat != null)
            {
                chat.Messages.Add(SystemMessage.UserLeftChat(DateTime.Now, mainVM.Client.UserName));
                mainVM.ChatClient.LeaveFromChatroom(mainVM.Client.SqlId, Chat.SqlId);
                Chat.CanWrite = false;
                chat.Messages.Clear();
                mainVM.Chats.Remove(chat);
                mainVM.SelectedChat = null;
            }
        }

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
                IsWritingText = dialog.FileName;
                InputMessage = new FileMessage(dialog.FileName);
            }
        }

        private void SendFile(object param)
        {
            FileMessage message = (FileMessage)inputMessage;

            if(message.FileName.Length > 0)
            {
                Chat.Messages.Add(new SessionSendedMessage(message));
                Scroll.ScrollToBottom();
            }

            InputMessage = new TextMessage();
        }

        private void CancelFile(object param)
        {
            InputMessage = new TextMessage();
        }

        private async void ShowMore(object param)
        {
            await LoadMore();
        }

        private void SendFile(string path)
        {
            string extn = path.Substring(path.LastIndexOf('.'));
            if (extn == ".mp3" || extn == ".wave")
            {
                Chat.Messages.Add(new UserMessage(new MediaMessage(path, DateTime.Now)));
                return;
            }

            ChatService.FileClient fileClient = new ChatService.FileClient();

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Guid stream_id = Guid.Empty;
            try
            {
                if (fileStream.CanRead)
                    stream_id = fileClient.FileUpload(Chat.SqlId, path, mainVM.Client.SqlId, fileStream);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
            Chat.Messages.Add(new SessionSendedMessage(new FileMessage(path, DateTime.Now, stream_id)));
        }

        private void MediaPosChanged(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (message == curMediaMessage)
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
        }

        private void Unload(object param)
        {
            if (Chat != null)
            {
                mainVM.ChatClient.MessageIsWriting(Chat.SqlId, null);
                if (curMediaMessage != null)
                {
                    curMediaMessage.IsPlaying = false;
                    curMediaMessage.CurrentLength = 0;
                    curMediaMessage = null;
                    player.Close();
                }
                chat.Messages.Clear();
                chat._messageOffset = 0;
                chat._offsetDate = DateTime.Now;
            }
        }

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}