using System.ComponentModel;
using System.Xml;

namespace Client.Models
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static Settings instance;
        private int messageLoadCount;
        private string selectedWallpaper;
        private Rington selectedRington;
        private bool notify;

        private Settings()
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            messageLoadCount = int.Parse(document.DocumentElement["MessageLoadCount"].InnerText);
            selectedWallpaper = document.DocumentElement["Wallpaper"].InnerText;
            notify = bool.Parse(document.DocumentElement["Notify"].InnerText);
            selectedRington = new Rington();
            selectedRington.Path = document.DocumentElement["Rington"].InnerText;
            selectedRington.Name = document.DocumentElement["RingtonName"].InnerText;

            document.Load("Settings/settings.xml");

            //подгрузака обоев
            int count = document.DocumentElement.ChildNodes[0].ChildNodes.Count;
            Wallpapers = new string[count];
            for (int i = 0; i < count; i++)
                Wallpapers[i] = document.DocumentElement.ChildNodes[0].ChildNodes[i].InnerText;

            //подгрузка рингтонов
            count = document.DocumentElement.ChildNodes[1].ChildNodes.Count;
            Ringtons = new Rington[count];
            for (int i = 0; i < count; i++)
            {
                Rington rington = new Rington();
                rington.Name = document.DocumentElement.ChildNodes[1].ChildNodes[i].Attributes["Name"].InnerText;
                rington.Path = document.DocumentElement.ChildNodes[1].ChildNodes[i].Attributes["Path"].InnerText;
                Ringtons[i] = rington;
                if (selectedRington.Name == rington.Name) rington.IsSelected = true;
            }
        }

        public static Settings Instance
        {
            get
            {
                if (instance == null) instance = new Settings();
                return instance;
            }
        }

        public int MessageLoadCount
        {
            get => messageLoadCount;
            set
            {
                Set(ref messageLoadCount, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["MessageLoadCount"].InnerText = messageLoadCount.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }
        public string[] Wallpapers { get; }
        public Rington[] Ringtons { get; }

        public string SelectedWallpaper
        {
            get => selectedWallpaper;
            set
            {
                Set(ref selectedWallpaper, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["Wallpaper"].InnerText = selectedWallpaper;
                document.Save("Settings/user-settings.xml");
            }
        }

        public Rington SelectedRington
        {
            get => selectedRington;
            set
            {
                Set(ref selectedRington, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["Rington"].InnerText = selectedRington.Path;
                document.DocumentElement["RingtonName"].InnerText = selectedRington.Name;
                document.Save("Settings/user-settings.xml");
            }
        }

        public bool Notify
        {
            get => notify;
            set
            {
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["Notify"].InnerText = notify.ToString();
                document.Save("Settings/user-settings.xml");
                Set(ref notify, value);
            }
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}
