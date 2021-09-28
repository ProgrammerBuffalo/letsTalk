using System;

namespace Client.Utility
{
    //class TicksToTime : System.Windows.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        long ticks = System.Convert.ToInt64(value);
    //        if (ticks == 0) return null;
    //        else if (ticks / 10000000 % 60 <= 9) return ticks / 600000000 + ":0" + ticks / 10000000 % 60;
    //        else return ticks / 600000000 + ":" + ticks / 10000000 % 60;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}

    class PathToExtension : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value.ToString();
            return path.Substring(path.LastIndexOf('.'));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class PathToName : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value.ToString();
            int start = path.LastIndexOf('\\');
            int end = path.LastIndexOf('.');
            if (start == -1) return path.Substring(0, end);
            else return path.Substring(start + 1, end - start - 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class FullPathConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return AppDomain.CurrentDomain.BaseDirectory + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class TypeConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    //class ColorConverter : System.Windows.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}

    class FitStringConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value.ToString();
            if (str.Length > 13) return value.ToString().Substring(0, 13) + "...";
            else return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class FitPathConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value.ToString();
            string name = path.Substring(0, path.LastIndexOf('.'));
            if (name.Length > 18) return name.ToString().Substring(0, 18) + "...";
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class FitDateConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            DateTime now = DateTime.Now;
            if (date.Day == now.Day && date.Month == now.Month && date.Year == now.Year) return date.ToString("HH:mm");
            else return date.ToString("dd/MM/yy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class EmojiConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string text = values[0].ToString();
            int fontSize = (int)values[1];

            System.Windows.Controls.TextBlock block = new System.Windows.Controls.TextBlock();
            block.TextWrapping = System.Windows.TextWrapping.Wrap;
            block.FontSize = fontSize;

            int i, start = 0;
            for (i = 0; i < text.Length; i++)
            {
                if (text[i] == '$' && text.Length - 5 >= i && text[i + 1] == '#')
                {
                    if (start != i) block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(start, i - start)) { BaselineAlignment = System.Windows.BaselineAlignment.TextTop });
                    string code = text.Substring(i, 5);
                    Models.Emoji emoji = Models.EmojiData.GetEmojiIcon(code);

                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Width = image.Height = fontSize + 5;
                    image.Margin = new System.Windows.Thickness(3, 0, 3, 0);
                    image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(emoji.Path, UriKind.Relative));
                    block.Inlines.Add(image);

                    i += 4;
                    start = i + 1;
                }
            }
            if (start != i) block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(start)) { BaselineAlignment = System.Windows.BaselineAlignment.TextTop });
            return block;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class FitEmojiConverter : System.Windows.Data.IValueConverter
    {
        public int FontSize { get; set; }
        public int ImageSize { get; set; }

        public object Convert(object value, Type targetType, object param, System.Globalization.CultureInfo culture)
        {
            string text = value.ToString();
            System.Windows.Controls.TextBlock block = new System.Windows.Controls.TextBlock();
            block.TextWrapping = System.Windows.TextWrapping.Wrap;
            block.FontSize = FontSize;
            int i, start = 0;
            for (i = 0; i < text.Length; i++)
            {
                if (text[i] == '$' && text.Length - 5 >= i && text[i + 1] == '#')
                {
                    if (start != i) block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(start, i - start)) { BaselineAlignment = System.Windows.BaselineAlignment.TextTop });
                    string code = text.Substring(i, 5);
                    Models.Emoji emoji = Models.EmojiData.GetEmojiIcon(code);

                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Width = image.Height = ImageSize;
                    image.Margin = new System.Windows.Thickness(3, 0, 3, 0);
                    image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(emoji.Path, UriKind.Relative));
                    block.Inlines.Add(image);

                    i += 4;
                    start = i + 1;
                }
            }
            if (start != i) block.Inlines.Add(new System.Windows.Documents.Run(text.Substring(start)) { BaselineAlignment = System.Windows.BaselineAlignment.TextTop });
            return block;
        }

        public object ConvertBack(object value, Type targetType, object param, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    class PathToUri : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object param, System.Globalization.CultureInfo culture)
        {
            return '/' + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object param, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
