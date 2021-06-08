using System;
using System.Windows.Documents;

//конверторы служат для предоставления данных в болле удобной форме для пользователя
//метод Convert преврашает данные из модели в данные для View 
//метод ConvertBack преврашает данные веденные из View в данные модели 
namespace Client.Utility
{
    //перевод из тиков во время для отброжения длины трека
    class TicksToTime : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long ticks = System.Convert.ToInt64(value);
            if (ticks == 0) return null;
            else if (ticks / 10000000 % 60 <= 9) return ticks / 600000000 + ":0" + ticks / 10000000 % 60;
            else return ticks / 600000000 + ":" + ticks / 10000000 % 60;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    //получение типа файла 
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

    //получение имени файл без полного пути
    class PathToName : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string path = value.ToString();
            return path.Substring(0, path.LastIndexOf('.'));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    //получени полного пути файла внутри сборки
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

    //выборка нужного цвета для никнеймов в груповых сообщениях
    class ColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    //конвертация строки с добовлением "..." в конец при большой длине строки
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

    class EmojiConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object param, System.Globalization.CultureInfo culture)
        {
            string text = value.ToString();
            System.Windows.Controls.TextBlock block = new System.Windows.Controls.TextBlock();
            block.TextWrapping = System.Windows.TextWrapping.Wrap;
            block.FontSize = 18;
            int i, start = 0;
            for (i = 0; i < text.Length; i++)
            {
                if (text[i] == '&' && text.Length - 5 >= i && text[i + 1] == '#')
                {
                    if (start != i) block.Inlines.Add(new Run(text.Substring(start, i - start)) { BaselineAlignment = System.Windows.BaselineAlignment.TextTop });
                    string code = text.Substring(i, 5);
                    Models.Emoji emoji = Models.EmojiData.GetEmoji(code);

                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Width = 30;
                    image.Height = 30;
                    image.Margin = new System.Windows.Thickness(3, 0, 3, 0);
                    image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(emoji.Path, UriKind.Relative));
                    block.Inlines.Add(image);

                    i += 4;
                    start = i + 1;
                }
            }
            if (start != i) block.Inlines.Add(new Run(text.Substring(start)) { BaselineAlignment = System.Windows.BaselineAlignment.TextTop });
            return block;
        }

        public object ConvertBack(object value, Type targetType, object param, System.Globalization.CultureInfo culture)
        {
            return value;
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
