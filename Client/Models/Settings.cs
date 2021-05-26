using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml;

namespace Client.Models
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static Settings instance;
        private int messageLoadCount;
        private string selectedWallpaper;
        private Rington selectedUserRington;
        private Rington selectedGroupRington;

        private bool canNotify;
        private bool canUserNotify;
        private bool canGroupNotify;

        private MediaPlayer player;

        private Settings()
        {
            player = new MediaPlayer();

            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            messageLoadCount = int.Parse(document.DocumentElement["MessageLoadCount"].InnerText);
            selectedWallpaper = document.DocumentElement["Wallpaper"].InnerText;
            canNotify = bool.Parse(document.DocumentElement["CanNotify"].InnerText);
            canUserNotify = bool.Parse(document.DocumentElement["CanUserNotify"].InnerText);
            canGroupNotify = bool.Parse(document.DocumentElement["CanGroupNotify"].InnerText);

            selectedUserRington = new Rington();
            selectedUserRington.Name = document.DocumentElement["UserRington"].Attributes["Name"].InnerText;
            selectedUserRington.Path = document.DocumentElement["UserRington"].Attributes["Path"].InnerText;

            selectedGroupRington = new Rington();
            selectedGroupRington.Name = document.DocumentElement["GroupRington"].Attributes["Name"].InnerText;
            selectedGroupRington.Path = document.DocumentElement["GroupRington"].Attributes["Path"].InnerText;
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

        public Rington SelectedUserRington
        {
            get => selectedUserRington;
            set
            {
                Set(ref selectedUserRington, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["UserRington"].Attributes["Name"].InnerText = value.Name;
                document.DocumentElement["UserRington"].Attributes["Path"].InnerText = value.Path;
                document.Save("Settings/user-settings.xml");
            }
        }

        public Rington SelectedGroupRington
        {
            get => selectedGroupRington;
            set
            {
                Set(ref selectedGroupRington, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["GroupRington"].Attributes["Name"].InnerText = value.Name;
                document.DocumentElement["GroupRington"].Attributes["Path"].InnerText = value.Path;
                document.Save("Settings/user-settings.xml");
            }
        }

        public bool CanNotify
        {
            get => canNotify;
            set
            {
                Set(ref canNotify, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["CanNotify"].InnerText = canNotify.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public bool CanUserNotify
        {
            get => canUserNotify;
            set
            {
                Set(ref canUserNotify, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["CanUserNotify"].InnerText = canUserNotify.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public bool CanGroupNotify
        {
            get => canGroupNotify;
            set
            {
                Set(ref canGroupNotify, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement["CanGroupNotify"].InnerText = canGroupNotify.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public IEnumerable<Rington> GetRingtons()
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/settings.xml");
            Rington[] ringtons = new Rington[document.DocumentElement["Ringtons"].ChildNodes.Count];
            for (int i = 0; i < ringtons.Length; i++)
            {
                Rington rington = new Rington();
                rington.Name = document.DocumentElement["Ringtons"].ChildNodes[i].Attributes["Name"].InnerText;
                rington.Path = document.DocumentElement["Ringtons"].ChildNodes[i].Attributes["Path"].InnerText;
                ringtons[i] = rington;
            }
            return ringtons;
        }

        public IEnumerable<string> GetWallpapers()
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/settings.xml");
            string[] paths = new string[document.DocumentElement["Wallpapers"].ChildNodes.Count];
            for (int i = 0; i < paths.Length; i++)
                paths[i] = document.DocumentElement["Wallpapers"].ChildNodes[i].InnerText;
            return paths;
        }

        private void AddNotify(XmlDocument document, int chatId, bool? canNotify)
        {
            XmlElement element = document.CreateElement("Notify");

            XmlAttribute attr = document.CreateAttribute("ChatId");
            attr.InnerText = chatId.ToString();
            element.Attributes.Append(attr);

            attr = document.CreateAttribute("CanNotify");
            if (canNotify == null) attr.InnerText = "";
            else attr.InnerText = canNotify.Value.ToString();
            element.Attributes.Append(attr);

            document.DocumentElement["Notifes"].AppendChild(element);
            document.Save("Settings/user-settings.xml");
        }

        public bool? GetMute(int chatId)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");
            foreach (XmlNode mute in document.DocumentElement["Notifes"])
            {
                if (int.Parse(mute.Attributes["ChatId"].InnerText) == chatId)
                {
                    if (mute.Attributes["CanNotify"].InnerText == "") return null;
                    else return new Nullable<bool>(bool.Parse(mute.Attributes["CanNotify"].InnerText));
                }
            }
            return new Nullable<bool>();
        }

        public void RemoveMute(int chatId)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");
            for (int i = 0; i < document.DocumentElement["Notifes"].ChildNodes.Count; i++)
            {
                if (int.Parse(document.DocumentElement["Notifes"].ChildNodes[i].Attributes["ChatId"].InnerText) == chatId)
                {
                    document.DocumentElement["Notifes"].RemoveChild(document.DocumentElement["Notify"].ChildNodes[i]);
                    document.Save("Settings/user-settings.xml");
                    return;
                }
            }
        }

        public void AddMute(int chatId, bool? isMute)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");
            for (int i = 0; i < document.DocumentElement["Notifes"].ChildNodes.Count; i++)
            {
                if (int.Parse(document.DocumentElement["Notifes"].ChildNodes[i].Attributes["ChatId"].InnerText) == chatId)
                {
                    if (isMute == null)
                        document.DocumentElement["Notifes"].ChildNodes[i].Attributes["CanNotify"].InnerText = "";
                    else
                        document.DocumentElement["Notifes"].ChildNodes[i].Attributes["CanNotify"].InnerText = isMute.Value.ToString();
                    document.Save("Settings/user-settings.xml");
                    return;
                }
            }
            AddNotify(document, chatId, isMute);
        }

        public void PlayRington(string path)
        {
            player.Close();
            player.Open(new Uri(path, UriKind.Relative));
            player.Play();
        }

        private void Set<T>(ref T prop, T value, [System.Runtime.CompilerServices.CallerMemberName] string prop_name = "")
        {
            prop = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }
    }
}