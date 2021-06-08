using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;

namespace Client.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _countLeft;

        private double _previousScrollOffset;

        private MainViewModel mainVM;
        private Chat chat;

        private MediaMessage curMediaMessage;

        private string isWritingText;
        private string messageText = "";

        private bool? canNotify;
        private System.Windows.Visibility loaderVisibility;
        private Message inputMessage;

        private EmojiGroup selectedEmojiGroup;
        private string searchEmojiText;
        private Emoji selectedEmoji;
        private ObservableCollection<Emoji> emojis;

        private System.Windows.Controls.ScrollViewer scroll;
        private System.Windows.Controls.RichTextBox rich;
        private Views.EmojiWindow window;

        public ChatViewModel(MainViewModel mainVM)
        {
            this.mainVM = mainVM;
            Chat = mainVM.SelectedChat;
            Chat.ClientId = mainVM.Client.SqlId;

            Settings = Settings.Instance;
            CanNotify = Settings.GetMute(chat.SqlId);

            InputMessage = new TextMessage();
            chat.Messages.Clear();

            chat._messageCount = 15;
            chat._messageOffset = 0;
            chat._offsetDate = DateTime.Now;
            _countLeft = chat._messageCount;

            InputMessage = new TextMessage();

            //player = new MediaPlayer();
            //player.MediaEnded += MediaEnded;

            //timer = new Timer();
            //timer.Elapsed += MediaPosTimer;
            //timer.Interval = 500;

            TextBox_KeyDownCommand = new Command(TextBox_KeyDown);
            TextBox_KeyUpCommand = new Command(TextBox_KeyUp);
            TextBox_EnterPressedCommand = new Command(TextBox_EnterPressed);
            TextBox_BackspacePressedCommand = new Command(TextBox_BackspacePressed);

            OpenFileCommand = new Command(OpenFile);
            CancelFileCommand = new Command(CancelFile);
            SendFileCommand = new Command(SendFile);

            OpenEmojiCommand = new Command(OpenEmoji);
            EmojiChangedCommand = new Command(EmojiChanged);
            EmojiGroupChangedCommand = new Command(EmojiGroupChanged);
            SearchEmojiTextChangedCommand = new Command(SearchEmojiTextChanged);
            FavEmojiCommand = new Command(FavEmoji);

            EditChatCommand = new Command(EditChat);
            LeaveChatCommand = new Command(LeaveChat);

            DownloadFileCommand = new Command(DownloadFile);

            CanNotifyChangedCommand = new Command(CanNotifyChanged);

            LoadCommand = new Command(Load);
            UnloadCommand = new Command(Unload);

            LoaderVisibility = System.Windows.Visibility.Visible;

            window = new Views.EmojiWindow();
            window.DataContext = this;
        }

        public ICommand TextBox_TextChangedCommand { get; set; }
        public ICommand TextBox_KeyDownCommand { get; }
        public ICommand TextBox_KeyUpCommand { get; }
        public ICommand TextBox_EnterPressedCommand { get; }
        public ICommand TextBox_BackspacePressedCommand { get; }

        public ICommand OpenEmojiCommand { get; }
        public ICommand EmojiGroupChangedCommand { get; }
        public ICommand EmojiChangedCommand { get; }
        public ICommand SearchEmojiTextChangedCommand { get; }
        public ICommand FavEmojiCommand { get; }

        public ICommand EditChatCommand { get; }
        public ICommand LeaveChatCommand { get; }

        public ICommand CanNotifyChangedCommand { get; }

        public ICommand OpenFileCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand CancelFileCommand { get; }

        public ICommand DownloadFileCommand { get; }

        public ICommand LoadCommand { get; }
        public ICommand UnloadCommand { get; }

        public Chat Chat { get => chat; set => Set(ref chat, value); }
        public Settings Settings { get; }

        public string IsWritingText { get => isWritingText; set => Set(ref isWritingText, value); }
        public Message InputMessage { get => inputMessage; set => Set(ref inputMessage, value); }
        public string MessageText { get => messageText; set => Set(ref messageText, value); }
        public bool? CanNotify { get => canNotify; set => Set(ref canNotify, value); }

        public EmojiGroup[] EmojiGroups { get => EmojiData.Groups; }
        public EmojiGroup SelectedEmojiGroup { get => selectedEmojiGroup; set => Set(ref selectedEmojiGroup, value); }
        public ObservableCollection<Emoji> Emojis { get => emojis; set => Set(ref emojis, value); }
        public Emoji SelectedEmoji { get => selectedEmoji; set => Set(ref selectedEmoji, value); }
        public string SearchEmojiText { get => searchEmojiText; set => Set(ref searchEmojiText, value); }

        public System.Windows.Visibility LoaderVisibility { get => loaderVisibility; set => Set(ref loaderVisibility, value); }
        public System.Windows.Controls.ScrollViewer Scroll { get; set; }

        public void SetScrollViewer(System.Windows.DependencyObject obj)
        {
            scroll = (System.Windows.Controls.ScrollViewer)obj;
            Scroll = scroll;
            Scroll.ScrollChanged += ScrollPositionChanged;
            Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
        }

        public void SetInputRichBox(System.Windows.DependencyObject obj)
        {
            rich = (System.Windows.Controls.RichTextBox)obj;
            rich.TextChanged += Rich_TextChanged;
        }

        private async void ScrollPositionChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (Scroll.VerticalOffset == 0 && _previousScrollOffset != Scroll.VerticalOffset)
            {
                _countLeft = chat._messageCount;
                _previousScrollOffset = Scroll.VerticalOffset;
                LoaderVisibility = System.Windows.Visibility.Visible;
                Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled;
                await LoadMore();
                Scroll.VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto;
                LoaderVisibility = System.Windows.Visibility.Collapsed;
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
            List<SourceMessage> sourceMessages = await Utility.MessageLoader.LoadMessage(this.chat, new List<SourceMessage>(), mainVM.Client.SqlId, 30, 30);

            if (LoaderVisibility.Equals(System.Windows.Visibility.Collapsed))
                foreach (var mes in sourceMessages)
                {
                    chat.Messages.Add(mes);
                }

        }

        private async void TextBox_EnterPressed(object obj)
        {
            if (String.IsNullOrEmpty(messageText)) return;
            await mainVM.ChatClient.SendMessageTextAsync(new ChatService.ServiceMessageText() { Text = MessageText, UserId = mainVM.Client.SqlId }, Chat.SqlId);
            Chat.Messages.Add(new UserMessage(new TextMessage(messageText, DateTime.Now)));
            Chat.LastMessage = new TextMessage(messageText, DateTime.Now);
            MessageText = null;
            if (mainVM.Chats.IndexOf(chat) != 0)
            {
                mainVM.Chats.Move(mainVM.Chats.IndexOf(chat), 0);
            }
            scroll.ScrollToBottom();
        }

        private void TextBox_BackspacePressed(object param)
        {
            int index = 0, point;
            point = -rich.CaretPosition.GetOffsetToPosition(rich.CaretPosition.DocumentStart) - 2;

            System.Windows.Documents.Paragraph paragraph = (System.Windows.Documents.Paragraph)rich.Document.Blocks.FirstBlock;

            foreach (var item in paragraph.Inlines)
            {
                if (item is System.Windows.Documents.Run)
                {
                    System.Windows.Documents.Run run = (System.Windows.Documents.Run)item;
                    for (int i = 0; i < run.Text.Length; i++, index++)
                    {
                        if (index == point - 1)
                        {
                            rich.TextChanged -= Rich_TextChanged;
                            if (!rich.Selection.IsEmpty)
                            {
                                int startSelect = -rich.Selection.Start.GetOffsetToPosition(rich.CaretPosition.DocumentStart);
                                int endSelect = -rich.Selection.End.GetOffsetToPosition(rich.CaretPosition.DocumentStart);
                                int count = rich.Selection.Start.GetOffsetToPosition(rich.Selection.End);

                                messageText = RemoveString(messageText, index - count, count);
                                rich.CaretPosition.DeleteTextInRun(count);
                            }
                            else
                            {
                                messageText = RemoveString(messageText, index);
                                rich.CaretPosition.DeleteTextInRun(-1);
                            }
                            rich.TextChanged += Rich_TextChanged;
                            return;
                        }
                    }
                }
                else if (item is System.Windows.Documents.InlineUIContainer)
                {
                    index += 5;
                }
            }
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

        private void Rich_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int index = 0, point = 0;
            point = -rich.CaretPosition.GetOffsetToPosition(rich.CaretPosition.DocumentStart) - 2;
            System.Windows.Documents.Paragraph paragraph = (System.Windows.Documents.Paragraph)rich.Document.Blocks.FirstBlock;
            if (paragraph != null)
            {
                foreach (var item in paragraph.Inlines)
                {
                    if (item is System.Windows.Documents.Run)
                    {
                        System.Windows.Documents.Run run = (System.Windows.Documents.Run)item;
                        run.BaselineAlignment = System.Windows.BaselineAlignment.TextTop;
                        for (int i = 0; i < run.Text.Length; i++, index++)
                        {
                            if (index == point - 1)
                            {
                                messageText = AddString(messageText, run.Text.Substring(i, 1), index);
                                return;
                            }
                        }
                    }
                    else if (item is System.Windows.Documents.InlineUIContainer)
                    {
                        index += 5;
                    }
                }
            }
        }

        private string AddString(string str, string add, int index)
        {
            if (str.Length == index)
            {
                return str + add;
            }
            else
            {
                string start = str.Substring(0, index);
                string end = str.Substring(index);
                return start + add + end;
            }
        }

        private string RemoveString(string str, int index, int count = 1)
        {
            if (str.Length == index)
            {
                return str.Remove(str.Length - 1, count);
            }
            else if (index < 0)
            {
                return str.Substring(count);
            }
            else
            {
                string start = str.Substring(0, index);
                string end = str.Substring(index + count);
                return start + end;
            }
        }

        private void CanNotifyChanged(object param)
        {
            Settings.AddMute(chat.SqlId, canNotify);
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
                Settings.RemoveMute(chat.SqlId);
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
                System.Windows.Media.Imaging.BitmapImage imageMessage = (message as ImageMessage).Bitmap;
                System.Windows.Media.Imaging.BitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(imageMessage));

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

            if (message.FileName.Length > 0)
            {
                Chat.Messages.Add(new SessionSendedMessage(message));
                Chat.LastMessage = new FileMessage(message.FileName, DateTime.Now);

                if (mainVM.Chats.IndexOf(chat) != 0)
                {
                    mainVM.Chats.Move(mainVM.Chats.IndexOf(chat), 0);
                }

                scroll.ScrollToBottom();
            }

            InputMessage = new TextMessage();
        }

        private void CancelFile(object param)
        {
            InputMessage = new TextMessage();
        }

        private void OpenEmoji(object param)
        {
            if (!window.IsActive)
            {
                window.Show();
            }
        }

        private void EmojiGroupChanged(object param)
        {
            if (SelectedEmojiGroup != null)
            {
                Emojis = new ObservableCollection<Emoji>(selectedEmojiGroup.Emojis);
            }
        }

        //messageText текс для сервера
        // ЗАМЕЧАНИЯ: 
        // При добавлении элементов в RichTextBox вызываеться TextChanged поэтому его надо отписывать от ивента
        private void EmojiChanged(object param)
        {
            if (SelectedEmoji != null)
            {
                rich.TextChanged -= Rich_TextChanged;
                System.Windows.Documents.Paragraph paragrap = (System.Windows.Documents.Paragraph)rich.Document.Blocks.FirstBlock;
                //позиция курсора минус 2 так как вначале самого документа есть 2 каких то escape символа в конце вроде тоже есть
                int point = -rich.CaretPosition.GetOffsetToPosition(rich.CaretPosition.DocumentStart) - 2;
                int index = 0, index2 = 0;

                //start emoji
                if (point == 0)
                {
                    messageText = AddString(messageText, selectedEmoji.Code, 0);
                    //чтобы поместить картинку в RictTextBox его надо обернуть в InlineUIContainer а потом добавить в Inlines 
                    //есть методы Add(добавляет в конец) InsertBefore(добвляет до выбраного элемента) InsertAfter (добвляет после выбраного элемента)
                    System.Windows.Documents.InlineUIContainer container = new System.Windows.Documents.InlineUIContainer(GetEmojiImage(selectedEmoji.Path));
                    paragrap.Inlines.InsertBefore(paragrap.Inlines.FirstInline, container);
                    //установка позиции каретки
                    rich.CaretPosition = container.ContentEnd;
                    rich.TextChanged += Rich_TextChanged;
                    SelectedEmoji = null;
                    return;
                }

                //middle text emoji
                var node = paragrap.Inlines.FirstInline;
                for (int i = 0; i < paragrap.Inlines.Count; i++)
                {
                    if (node is System.Windows.Documents.Run)
                    {
                        System.Windows.Documents.Run run = (System.Windows.Documents.Run)node;
                        if (point < index + run.Text.Length)
                        {
                            index2 = point - index;
                            index += index2;
                            messageText = AddString(messageText, selectedEmoji.Code, index);

                            System.Windows.Documents.InlineUIContainer container = new System.Windows.Documents.InlineUIContainer(GetEmojiImage(selectedEmoji.Path));
                            paragrap.Inlines.InsertAfter(run, container);

                            if (index2 == run.Text.Length)
                            {
                                rich.CaretPosition = container.ContentEnd;
                            }
                            else
                            {
                                string end = run.Text.Substring(index2);
                                run.Text = run.Text.Substring(0, index2);
                                run = new System.Windows.Documents.Run(end);
                                run.BaselineAlignment = System.Windows.BaselineAlignment.TextTop;
                                paragrap.Inlines.InsertAfter(container, run);
                                rich.CaretPosition = run.ContentStart;
                            }

                            rich.TextChanged += Rich_TextChanged;
                            SelectedEmoji = null;
                            return;
                        }
                        else
                        {
                            index += run.Text.Length;
                            index2 += run.Text.Length;
                        }
                    }
                    else if (node is System.Windows.Documents.InlineUIContainer)
                    {
                        index += 5;
                        index2 += 5;
                        //end emoji
                        if (point == index2)
                        {
                            messageText = AddString(messageText, selectedEmoji.Code, index);
                            System.Windows.Documents.InlineUIContainer container = new System.Windows.Documents.InlineUIContainer(GetEmojiImage(selectedEmoji.Path));
                            paragrap.Inlines.InsertAfter(node, container);
                            rich.CaretPosition = container.ContentEnd;
                            rich.TextChanged += Rich_TextChanged;
                            SelectedEmoji = null;
                            return;
                        }
                    }
                    node = node.NextInline;
                }
                //empty text emoji   
                messageText = AddString(messageText, selectedEmoji.Code, index);
                System.Windows.Documents.InlineUIContainer _container = new System.Windows.Documents.InlineUIContainer(GetEmojiImage(selectedEmoji.Path));
                paragrap.Inlines.Add(_container);
                rich.CaretPosition = _container.ContentEnd;
                rich.TextChanged += Rich_TextChanged;
                SelectedEmoji = null;
            }
        }

        private System.Windows.Controls.Image GetEmojiImage(string path)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Width = 35;
            image.Height = 35;
            image.Margin = new System.Windows.Thickness(3, 5, 3, 0);
            image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.Relative));
            return image;
        }

        private void SearchEmojiTextChanged(object param)
        {
            if (!String.IsNullOrWhiteSpace(SearchEmojiText))
            {
                Emojis = new ObservableCollection<Emoji>();
                Task.Run(() =>
                {
                    foreach (var group in EmojiData.Groups)
                    {
                        foreach (var emoji in group.Emojis)
                        {
                            if (StringExtensions.ContainsAtStart(emoji.Name, searchEmojiText))
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Emojis.Add(emoji);
                                });
                            }
                        }
                    }
                });
            }
            else if (selectedEmojiGroup != null)
            {
                Emojis = new ObservableCollection<Emoji>(selectedEmojiGroup.Emojis);
            }
        }

        private void FavEmoji(object param)
        {

        }

        private async void ShowMore(object param)
        {
            await LoadMore();
        }

        //private async void Load(object obj)
        //{
        //    await LoadMore();
        //    Scroll.ScrollToEnd();
        //}

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
                    //player.Close();
                }
                chat.Messages.Clear();
                chat._messageOffset = 0;
                chat._offsetDate = DateTime.Now;
            }
        }

        //private async Task LoadMore()
        //{
        //    await Utility.MessageLoader.LoadMessage(this.Chat, mainVM.Client.SqlId, 30, 30);
        //}

        public void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}

//private void TextInput()
//{
//    var paragrah = (System.Windows.Documents.Paragraph)((System.Windows.Documents.FlowDocument)rich.Document).Blocks.FirstBlock;
//    foreach (var item in paragrah.Inlines)
//    {
//        if (item is System.Windows.Documents.Run)
//        {
//            System.Windows.Documents.Run run = (System.Windows.Documents.Run)item;
//        }
//        else if (item is System.Windows.Documents.InlineUIContainer)
//        {
//            System.Windows.Controls.Image image = (System.Windows.Controls.Image)((System.Windows.Documents.InlineUIContainer)item).Child;
//        }
//    }
//}

//public ICommand MediaPlayCommand { get; }
//public ICommand MediaPosChangedCommand { get; }
//public ICommand SendCommand { get; }

//MediaPlayCommand = new Command(MediaPlay);
//MediaPosChangedCommand = new Command(MediaPosChanged);
//SendCommand = new Command(Send);

//private void MediaEnded(object sender, EventArgs e)
//{
//    player.Close();
//    timer.Stop();
//    curMediaMessage.CurrentLength = 0;
//    curMediaMessage.IsPlaying = false;
//}

//private void MediaPosTimer(object sender, EventArgs e)
//{
//    App.Current.Dispatcher.Invoke(() =>
//    {
//        curMediaMessage.CurrentLength = player.Position.Ticks;
//    });
//}

//private void MediaPosChanged(object param)
//{
//    MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
//    if (message == curMediaMessage)
//        player.Position = TimeSpan.FromTicks(message.CurrentLength);
//}

//private void MediaPlay(object param)
//{
//    MediaMessage message = (MediaMessage)((SourceMessage)param).Message;
//    if (player.Source == null)
//    {
//        curMediaMessage = message;
//        player.Open(new Uri(message.FileName, UriKind.Absolute));
//        player.Position = TimeSpan.FromTicks(message.CurrentLength);
//        player.Play();
//        timer.Start();
//        curMediaMessage.IsPlaying = true;
//    }
//    else if (message != curMediaMessage)
//    {
//        curMediaMessage.IsPlaying = false;
//        curMediaMessage.CurrentLength = 0;
//        player.Close();
//        player.Open(new Uri(message.FileName, UriKind.Absolute));
//        player.Position = TimeSpan.FromTicks(message.CurrentLength);
//        player.Play();
//        timer.Start();
//        curMediaMessage = message;
//        curMediaMessage.IsPlaying = true;
//    }
//    else if (curMediaMessage.IsPlaying)
//    {
//        player.Pause();
//        timer.Stop();
//        curMediaMessage.IsPlaying = false;
//    }
//    else
//    {
//        player.Play();
//        timer.Start();
//        curMediaMessage.IsPlaying = true;
//    }
//}