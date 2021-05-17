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
            if (str.Length > 20) return value.ToString().Substring(0, 20) + "...";
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
            if (date.Day == now.Day && date.Month == now.Month && date.Year == now.Year) return date.ToString("hh:mm");
            else return date.ToString("dd/mm/yy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
