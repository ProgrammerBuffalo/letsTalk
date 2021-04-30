using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Client.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //нужен чтобы понять какой вид сообщения пошлет user;
        private delegate void MessageSendType(string message);

        private ClientUserInfo client;

        private MessageSendType sendType;

        private Chat chat;

        private MediaMessage curMediaMessage;

        private MediaPlayer player;

        private Timer timer;

        private string isWritingText;

        private string messageText = "";

        public ChatService.ChatClient ChatClient { get; set; }

        public ChatViewModel(ChatService.ChatClient chatClient)
        {
            client = ClientUserInfo.getInstance();
            ChatClient = chatClient;

            TextBox_KeyDownCommand = new Command(TextBox_KeyDown);
            TextBox_KeyUpCommand = new Command(TextBox_KeyUp);
            TextBox_EnterPressedCommand = new Command(TextBox_EnterPressed);

            MediaPlayCommand = new Command(MediaPlay);
            MediaPosChangedCommand = new Command(MediaPosChanged);
            SendCommand = new Command(Send);
            ShowMoreCommand = new Command(ShowMore);
            OpenFileCommand = new Command(OpenFile);
            UnloadCommand = new Command(Unload);
            LoadCommand = new Command(Load);

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

        private void Load(object obj)
        {
            LoadMore();
        }

        private async void LoadMore()
        {
            ChatService.UnitClient unitClient = new ChatService.UnitClient();
            ChatService.ServiceMessage[] serviceMessages = await unitClient.MessagesFromOneChatAsync(chat.SqlId, chat._messageOffset, chat._messageCount);
            if (serviceMessages == null)
                return;

            ObservableCollection<Models.SourceMessage> messages =
                new ObservableCollection<Models.SourceMessage>(await System.Threading.Tasks.Task<List<SourceMessage>>.Run(() =>
                {
                    List<SourceMessage> messagesFromChat = new List<SourceMessage>();
                    foreach (var message in serviceMessages)
                    {
                        if (message is ChatService.ServiceMessageText)
                        {
                            var textmessage = message as ChatService.ServiceMessageText;
                            messagesFromChat.Add(chat.GetMessageType(textmessage.UserId, new TextMessage(textmessage.Text, textmessage.DateTime)));
                        }
                        else
                        {
                            var filemessage = message as ChatService.ServiceMessageFile;
                            messagesFromChat.Add(chat.GetMessageType(filemessage.UserId, new FileMessage(filemessage.FileName, filemessage.DateTime, filemessage.StreamId) { IsLoaded = true }));
                        }

                    }

                    return messagesFromChat;
                }));

            foreach (var message in messages)
                chat.Messages.Insert(0, message);

            chat._messageOffset += chat._messageCount;
        }

        private void TextBox_EnterPressed(object obj)
        {
            if (MessageText.Length < 1)
                return;
            ChatClient.SendMessageTextAsync(new ChatService.ServiceMessageText() { Text = MessageText, UserId = client.SqlId }, chat.SqlId);
            chat.Messages.Add(chat.GetMessageType(client.SqlId, new TextMessage(MessageText, DateTime.Now)));
            MessageText = "";
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
        }

        public ChatViewModel(Chat chat, ChatService.ChatClient chatClient) : this(chatClient)
        {
            Chat = chat;
        }

        public MainViewModel.ChatDelegate RemoveChat { get; set; }

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

        public ICommand TextBox_KeyDownCommand { get; }
        public ICommand TextBox_KeyUpCommand { get; }
        public ICommand TextBox_EnterPressedCommand { get; }

        public ICommand MediaPlayCommand { get; }
        public ICommand MediaPosChangedCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand ShowMoreCommand { get; }
        public ICommand UnloadCommand { get; }
        public ICommand LoadCommand { get; }

        public ICommand AddUserCommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand LeaveChatCommand { get; }
        public ICommand DeleteChatCommand { get; }

        public ICommand DownloadFileCommand { get; }

        public Chat Chat { get => chat; set => Set(ref chat, value); }
        public string IsWritingText { get => isWritingText; set => Set(ref isWritingText, value); }
        public string MessageText { get => messageText; set => Set(ref messageText, value); }


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
            if(message is ImageMessage)
            {
                BitmapImage imageMessage = (message as ImageMessage).Bitmap;

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(imageMessage));

                using (var filestream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
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
                chat.Messages.Add(chat.GetMessageType(client.SqlId, new MediaMessage(path, DateTime.Now)));
                return;
            }

            ChatService.FileClient fileClient = new ChatService.FileClient();

            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Guid stream_id = Guid.Empty;
            try
            {
                if (fileStream.CanRead)
                    stream_id = fileClient.FileUpload(chat.SqlId, path, client.SqlId, fileStream);
            }
            catch (Exception ex) { }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
            chat.Messages.Add(new SessionSendedMessage(new FileMessage(path, DateTime.Now, stream_id)));
        }

        private void MediaPosChanged(object param)
        {
            MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
            if (message == curMediaMessage)
                player.Position = TimeSpan.FromTicks(message.CurrentLength);
        }

        private void Unload(object param)
        {
            ChatClient.MessageIsWriting(Chat.SqlId, null);
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

