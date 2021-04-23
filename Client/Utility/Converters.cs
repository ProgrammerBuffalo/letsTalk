using System;

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

    //получение типо файла 
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
            return path.Substring(path.LastIndexOf('/') + 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    //для зеленого кружка в ChatItem если значение count больше чем 99 то получаем "99+" 
    class CountConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int count = System.Convert.ToInt32(value);
            if (count > 99) return "99+";
            else return count;
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
}
