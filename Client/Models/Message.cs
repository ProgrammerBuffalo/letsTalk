using System;
using System.ComponentModel;

namespace Client.Models
{
    //сюда можешь добавить Id для сообшений
    public class Message : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime date;

        public Message()
        {
            Date = DateTime.Now;
        }

        public Message(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get => date; set => Set(ref date, value); }

        protected void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }

    public class TextMessage : Message
    {
        private string text;

        public TextMessage()
        {

        }

        public TextMessage(string text)
        {
            Text = text;
        }

        public TextMessage(string text, DateTime date) : this(text)
        {
            Date = date;
        }

        public string Text { get => text; set => Set(ref text, value); }
    }

    public class FileMessage : Message
    {
        private static string root;
        private string path;

        static FileMessage()
        {
            root = AppDomain.CurrentDomain.BaseDirectory;
        }

        public FileMessage()
        {

        }

        public FileMessage(string path)
        {
            Path = root + path;
        }

        public FileMessage(string path, DateTime date) : this(path)
        {
            Date = date;
        }

        public string Path { get => path; set => Set(ref path, value); }
    }

    public class MediaMessage : FileMessage
    {
        private long length;
        private long currentLength;
        private bool isPlaying;

        public MediaMessage(string path) : base(path)
        {
            using (var shell = Microsoft.WindowsAPICodePack.Shell.ShellFile.FromFilePath(path))
            {
                Length = (long)shell.Properties.System.Media.Duration.Value.Value;
            }
        }

        public MediaMessage(string path, DateTime date) : this(path)
        {
            Date = date;
        }

        public long Length { get => length; set => Set(ref length, value); }
        public long CurrentLength { get => currentLength; set => Set(ref currentLength, value); }
        public bool IsPlaying { get => isPlaying; set => Set(ref isPlaying, value); }
    }


}
