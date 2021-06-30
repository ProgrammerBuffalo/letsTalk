using Client.Models;
using Client.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _countLeft;

        private double _previousScrollOffset;

        private MainViewModel mainVM;
        private Chat chat;

        private string isWritingText;
        private string messageText = "";

        private bool? canNotify;
        private System.Windows.Visibility loaderVisibility;
        private Message inputMessage;

        private Views.EmojiWindow emojiWindow;
        private EmojiGroup selectedEmojiGroup;
        private string searchEmojiText;
        private Emoji selectedEmoji;
        private ObservableCollection<Emoji> emojis;

        private System.Windows.Controls.RichTextBox rich;
        private System.Windows.Controls.ScrollViewer scroll;

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

            TextBox_KeyDownCommand = new Command(TextBox_KeyDown);
            TextBox_KeyUpCommand = new Command(TextBox_KeyUp);
            TextBox_EnterPressedCommand = new Command(TextBox_EnterPressed);

            OpenFileCommand = new Command(OpenFile);
            CancelFileCommand = new Command(CancelFile);
            SendFileCommand = new Command(SendFile);
            DownloadFileCommand = new Command(DownloadFile);

            EditChatCommand = new Command(EditChat);
            LeaveChatCommand = new Command(LeaveChat);

            CanNotifyChangedCommand = new Command(CanNotifyChanged);

            OpenEmojisCommand = new Command(OpenEmojis);
            EmojiChangedCommand = new Command(EmojiChanged);
            EmojiGroupChangedCommand = new Command(EmojiGroupChanged);
            SearchEmojiTextChangedCommand = new Command(SearchEmojiTextChanged);
            FavEmojisCommand = new Command(FavEmojis);

            LoadCommand = new Command(Load);
            UnloadCommand = new Command(Unload);

            emojiWindow = new Views.EmojiWindow();
            emojiWindow.DataContext = this;
            LoaderVisibility = System.Windows.Visibility.Visible;
        }

        public ICommand TextBox_KeyDownCommand { get; }
        public ICommand TextBox_KeyUpCommand { get; }
        public ICommand TextBox_EnterPressedCommand { get; }

        public ICommand EditChatCommand { get; }
        public ICommand LeaveChatCommand { get; }

        public ICommand CanNotifyChangedCommand { get; }

        public ICommand OpenFileCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand CancelFileCommand { get; }

        public ICommand DownloadFileCommand { get; }

        public ICommand OpenEmojisCommand { get; }
        public ICommand EmojiGroupChangedCommand { get; }
        public ICommand EmojiChangedCommand { get; }
        public ICommand SearchEmojiTextChangedCommand { get; }
        public ICommand FavEmojisCommand { get; }

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
            rich.Loaded += Rich_Loaded;
        }

        private void Rich_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            rich.TextChanged += rich_TextChanged;
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
            List<SourceMessage> sourceMessages = await MessageLoader.LoadMessage(this.chat, new List<SourceMessage>(), mainVM.Client.SqlId, 30, 30);

            //if (LoaderVisibility.Equals(System.Windows.Visibility.Collapsed))
            //    foreach (var mes in sourceMessages)
            //    {
            //        chat.Messages.Add(mes);
            //    }
            foreach (var mes in sourceMessages)
            {
                chat.Messages.Add(mes);
            }
        }

        private async void TextBox_EnterPressed(object obj)
        {
            if (String.IsNullOrEmpty(messageText)) return;
            await mainVM.ChatClient.SendMessageTextAsync(new ChatService.ServiceMessageText() { Text = messageText, UserId = mainVM.Client.SqlId }, Chat.SqlId);
            Chat.Messages.Add(new UserMessage(new TextMessage(messageText, DateTime.Now)));
            Chat.LastMessage = new TextMessage(messageText, DateTime.Now);
            MessageText = "";
            (rich.Document.Blocks.FirstBlock as System.Windows.Documents.Paragraph).Inlines.Clear();
            if (mainVM.Chats.IndexOf(chat) != 0)
            {
                mainVM.Chats.Move(mainVM.Chats.IndexOf(chat), 0);
            }
            scroll.ScrollToBottom();
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
                await Task.Run(() =>
                {
                    string name = fileClient.FileDownload(message.StreamId, out lenght, out stream);
                    if (lenght <= 0)
                        return;
                    memoryStream = FileHelper.ReadFileByPart(stream);

                    fileStream = new FileStream(filename, FileMode.Create);
                    memoryStream.CopyTo(fileStream);
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
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

        private async void ShowMore(object param)
        {
            await LoadMore();
        }

        private System.Windows.Documents.InlineUIContainer GetEmojiInlineContainer(string path, string code)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Width = 35;
            image.Height = 35;
            image.Margin = new System.Windows.Thickness(3, 5, 3, 0);
            image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(path, UriKind.Relative));
            System.Windows.Documents.InlineUIContainer container = new System.Windows.Documents.InlineUIContainer(image);
            container.Tag = code;
            return container;
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

        private void FavEmojis(object param)
        {
            SelectedEmojiGroup = null;
            Emojis = new ObservableCollection<Emoji>(Settings.GetFavEmojis());
        }

        private void OpenEmojis(object param)
        {
            if (!emojiWindow.IsActive) emojiWindow.Show();
        }

        private void EmojiGroupChanged(object param)
        {
            if (SelectedEmojiGroup != null)
            {
                Emojis = new ObservableCollection<Emoji>(selectedEmojiGroup.Emojis);
            }
        }

        private void EmojiChanged(object param)
        {
            if (selectedEmoji == null) return;

            int point, index = 0, index2 = 0;
            point = new System.Windows.Documents.TextRange(rich.Document.ContentStart, rich.CaretPosition).Text.Length;
            rich.TextChanged -= rich_TextChanged;

            System.Windows.Documents.Paragraph par = (System.Windows.Documents.Paragraph)rich.Document.Blocks.FirstBlock;
            var next = par.Inlines.FirstInline;
            for (int i = 0; i < par.Inlines.Count; i++)
            {
                if (next is System.Windows.Documents.Run)
                {
                    System.Windows.Documents.Run run = next as System.Windows.Documents.Run;
                    if (point <= index + run.Text.Length)
                    {
                        System.Windows.Documents.InlineUIContainer container = GetEmojiInlineContainer(selectedEmoji.Path, selectedEmoji.Code);
                        index = run.Text.Length - (index + run.Text.Length) + point;
                        if (index == run.Text.Length)
                        {
                            index2 += run.Text.Length;
                            par.Inlines.InsertAfter(run, container);
                        }
                        else
                        {
                            index2 += index;
                            string prev = run.Text.Substring(0, index);
                            string after = run.Text.Substring(index);
                            run.Text = prev;
                            par.Inlines.InsertAfter(run, container);
                            System.Windows.Documents.Run _run = new System.Windows.Documents.Run(after);
                            _run.BaselineAlignment = System.Windows.BaselineAlignment.Center;
                            par.Inlines.InsertAfter(container, _run);
                        }
                        rich.CaretPosition = container.ContentEnd;
                        messageText = messageText.Insert(index2, selectedEmoji.Code);
                        rich.TextChanged += rich_TextChanged;
                        Settings.AddFavEmoji(selectedEmoji.Code);
                        SelectedEmoji = null;
                        return;
                    }
                    else
                    {
                        index += run.Text.Length;
                        index2 += run.Text.Length;
                    }
                }
                else
                {
                    index++;
                    index2 += 5;
                    if (index == point)
                    {
                        System.Windows.Documents.InlineUIContainer container = GetEmojiInlineContainer(selectedEmoji.Path, selectedEmoji.Code);
                        par.Inlines.InsertAfter(next, container);
                        rich.CaretPosition = container.ContentEnd;
                        rich.TextChanged += rich_TextChanged;
                        messageText = messageText.Insert(index2, selectedEmoji.Code);
                        Settings.AddFavEmoji(selectedEmoji.Code);
                        SelectedEmoji = null;
                        return;
                    }
                }
                next = next.NextInline;
            }
            System.Windows.Documents.InlineUIContainer _container = GetEmojiInlineContainer(selectedEmoji.Path, selectedEmoji.Code);
            par.Inlines.Add(_container);
            rich.CaretPosition = _container.ContentEnd;
            rich.TextChanged += rich_TextChanged;
            messageText = messageText.Insert(index2, "&#001");
            Settings.AddFavEmoji(selectedEmoji.Code);
            SelectedEmoji = null;
        }


        private void rich_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            StringBuilder @string = new StringBuilder();
            System.Windows.Documents.Paragraph par = (System.Windows.Documents.Paragraph)rich.Document.Blocks.FirstBlock;
            foreach (var item in par.Inlines)
            {
                if (item is System.Windows.Documents.Run)
                {
                    System.Windows.Documents.Run run = (System.Windows.Documents.Run)item;
                    run.BaselineAlignment = System.Windows.BaselineAlignment.Center;
                    @string.Append(run.Text);
                }
                else
                {
                    System.Windows.Documents.InlineUIContainer image = (System.Windows.Documents.InlineUIContainer)item;
                    @string.Append(image.Tag);
                }
            }
            messageText = @string.ToString();
        }

        private void Unload(object param)
        {
            if (Chat != null)
            {
                mainVM.ChatClient.MessageIsWriting(Chat.SqlId, null);
                chat.Messages.Clear();
                chat._messageOffset = 0;
                chat._offsetDate = DateTime.Now;
            }
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}