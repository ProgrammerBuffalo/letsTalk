﻿using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Client.Models
{
    public class Message : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected DateTime date;

        public Message()
        {
            Date = DateTime.Now;
        }

        public Message(DateTime date)
        {
            Date = date;
        }

        public DateTime Date { get => date; set => Set(ref date, value); }

        public virtual object Clone()
        {
            return new Message(date);
        }

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

        public override object Clone()
        {
            return new TextMessage(text, date);
        }
    }

    public class FileMessage : Message
    {
        protected string fileName;
        protected bool isLoaded;

        public FileMessage()
        {
            IsLoaded = false;
        }

        public FileMessage(string fileName) : this()
        {
            FileName = fileName;
        }

        public FileMessage(string fileName, DateTime date) : this(fileName)
        {
            Date = date;
        }

        public FileMessage(string fileName, DateTime date, Guid streamId) : this(fileName, date)
        {
            StreamId = streamId;
        }

        public FileMessage(string fileName, DateTime date, Guid streamdId, bool isLoaded) : this(fileName, date, streamdId)
        {
            IsLoaded = isLoaded;
        }

        public Guid StreamId { get; set; }
        public string FileName { get => fileName; set => Set(ref fileName, value); }
        public bool IsLoaded { get => isLoaded; set => Set(ref isLoaded, value); }

        public override object Clone()
        {
            return new FileMessage(fileName, date, StreamId, isLoaded);
        }
    }

    public class ImageMessage : FileMessage
    {
        private BitmapImage bitmap;
        public BitmapImage Bitmap { get => bitmap; set => Set(ref bitmap, value); }

        public ImageMessage(string fileName, DateTime date, Guid streamId) : base(fileName, date, streamId) { }

        public ImageMessage(string fileName, DateTime date, Guid streamId, BitmapImage bitmap) : this(fileName, date, streamId)
        {
            Bitmap = bitmap;
        }

        public ImageMessage(string fileName, DateTime date, Guid streamId, bool isLoaded, BitmapImage image) : base(fileName, date, streamId, isLoaded)
        {
            Bitmap = bitmap;
        }

        public override object Clone()
        {
            return new ImageMessage(fileName, date, StreamId, bitmap);
        }
    }

}
