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
        private int userId;
        private int userNodeIndex;
        private int favEmojiCount;

        private MediaPlayer player;

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Settings();
                    instance.player = new MediaPlayer();
                    //instance.Temp();
                }
                return instance;
            }
        }

        private void Temp()
        {
            AddFavEmoji("a");
            AddFavEmoji("b");
            AddFavEmoji("a");
            AddFavEmoji("c");
            AddFavEmoji("d");
            AddFavEmoji("d");
            AddFavEmoji("d");
            AddFavEmoji("d");
            AddFavEmoji("d");
            AddFavEmoji("d");
        }

        public int MessageLoadCount
        {
            get => messageLoadCount;
            set
            {
                Set(ref messageLoadCount, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement.ChildNodes[userNodeIndex]["MessageLoadCount"].InnerText = messageLoadCount.ToString();
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
                document.DocumentElement.ChildNodes[userNodeIndex]["Wallpaper"].InnerText = selectedWallpaper;
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
                document.DocumentElement.ChildNodes[userNodeIndex]["UserRington"].Attributes["Name"].InnerText = value.Name;
                document.DocumentElement.ChildNodes[userNodeIndex]["UserRington"].Attributes["Path"].InnerText = value.Path;
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
                document.DocumentElement.ChildNodes[userNodeIndex]["GroupRington"].Attributes["Name"].InnerText = value.Name;
                document.DocumentElement.ChildNodes[userNodeIndex]["GroupRington"].Attributes["Path"].InnerText = value.Path;
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
                document.DocumentElement.ChildNodes[userNodeIndex]["CanNotify"].InnerText = canNotify.ToString();
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
                document.DocumentElement.ChildNodes[userNodeIndex]["CanUserNotify"].InnerText = canUserNotify.ToString();
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
                document.DocumentElement.ChildNodes[userNodeIndex]["CanGroupNotify"].InnerText = canGroupNotify.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public void LoadSettings(int userId)
        {
            this.userId = userId;
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            for (int i = 0; i < document.DocumentElement.ChildNodes.Count; i++)
            {
                if (int.Parse(document.DocumentElement.ChildNodes[i].Attributes["UserId"].InnerText) == userId)
                {
                    this.userNodeIndex = i;

                    messageLoadCount = int.Parse(document.DocumentElement.ChildNodes[i]["MessageLoadCount"].InnerText);
                    selectedWallpaper = document.DocumentElement.ChildNodes[i]["Wallpaper"].InnerText;
                    canNotify = bool.Parse(document.DocumentElement.ChildNodes[i]["CanNotify"].InnerText);
                    canUserNotify = bool.Parse(document.DocumentElement.ChildNodes[i]["CanUserNotify"].InnerText);
                    canGroupNotify = bool.Parse(document.DocumentElement.ChildNodes[i]["CanGroupNotify"].InnerText);

                    selectedUserRington = new Rington();
                    selectedUserRington.Name = document.DocumentElement.ChildNodes[i]["UserRington"].Attributes["Name"].InnerText;
                    selectedUserRington.Path = document.DocumentElement.ChildNodes[i]["UserRington"].Attributes["Path"].InnerText;

                    selectedGroupRington = new Rington();
                    selectedGroupRington.Name = document.DocumentElement.ChildNodes[i]["GroupRington"].Attributes["Name"].InnerText;
                    selectedGroupRington.Path = document.DocumentElement.ChildNodes[i]["GroupRington"].Attributes["Path"].InnerText;
                    break;
                }
            }
        }

        public void AddUser(int userId)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            this.userId = userId;

            XmlElement user = document.CreateElement("User");
            XmlAttribute attribute = document.CreateAttribute("UserId");
            attribute.InnerText = userId.ToString();
            user.Attributes.Append(attribute);

            XmlElement element = document.CreateElement("MessageLoadCount");
            element.InnerText = "30";
            user.AppendChild(element);

            element = document.CreateElement("Wallpaper");
            element.InnerText = "/Resources/Wallpapers/skin.jpg";
            user.AppendChild(element);

            element = document.CreateElement("CanNotify");
            element.InnerText = "True";
            user.AppendChild(element);

            element = document.CreateElement("CanUserNotify");
            element.InnerText = "True";
            user.AppendChild(element);

            element = document.CreateElement("CanGroupNotify");
            element.InnerText = "True";
            user.AppendChild(element);

            element = document.CreateElement("UserRington");
            attribute = document.CreateAttribute("Name");
            attribute.InnerText = "Hello";
            element.Attributes.Append(attribute);
            attribute = document.CreateAttribute("Path");
            attribute.InnerText = "/Resources/Ringtons/hello.wav";
            element.Attributes.Append(attribute);
            user.AppendChild(element);

            element = document.CreateElement("GroupRington");
            attribute = document.CreateAttribute("Name");
            attribute.InnerText = "Hello";
            element.Attributes.Append(attribute);
            attribute = document.CreateAttribute("Path");
            attribute.InnerText = "/Resources/Ringtons/hello.wav";
            element.Attributes.Append(attribute);
            user.AppendChild(element);

            element = document.CreateElement("Notifes");
            user.AppendChild(element);

            document.DocumentElement.AppendChild(user);
            document.Save("Settings/user-settings.xml");
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

            document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].AppendChild(element);
            document.Save("Settings/user-settings.xml");
        }

        public bool? GetMute(int chatId)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");
            foreach (XmlNode mute in document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"])
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
            for (int i = 0; i < document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes.Count; i++)
            {
                if (int.Parse(document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes[i].Attributes["ChatId"].InnerText) == chatId)
                {
                    document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].RemoveChild(document.DocumentElement["Notify"].ChildNodes[i]);
                    document.Save("Settings/user-settings.xml");
                    return;
                }
            }
        }

        public void AddMute(int chatId, bool? isMute)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");
            for (int i = 0; i < document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes.Count; i++)
            {
                if (int.Parse(document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes[i].Attributes["ChatId"].InnerText) == chatId)
                {
                    if (isMute == null) document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes[i].Attributes["CanNotify"].InnerText = "";
                    else document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes[i].Attributes["CanNotify"].InnerText = isMute.Value.ToString();
                    document.Save("Settings/user-settings.xml");
                    return;
                }
            }
            AddNotify(document, chatId, isMute);
        }

        private void AddFavEmoji(string emoji)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            foreach (XmlNode item in document.DocumentElement.ChildNodes[userId]["FavEmojis"])
            {
                if (item.Attributes["Text"].InnerText == emoji)
                {
                    int tempCount = int.Parse(item.Attributes["Count"].InnerText) + 1;
                    string tempText = item.Attributes["Text"].InnerText;
                    //foreach (XmlNode item2 in document.DocumentElement["FavEmoji"])
                    //{
                    //    if(int.Parse(item2.Attributes["Count"].InnerText) <= tempCount)
                    //    {
                    //        document.DocumentElement["FavEmojis"].ChildNodes[0].InsertAfter(null, null);
                    //    }
                    //}
                    break;
                }
            }
            XmlElement element = document.CreateElement("Emoji");
            XmlAttribute attr = document.CreateAttribute("Text");
            attr.InnerText = emoji;
            element.Attributes.Append(attr);

            attr = document.CreateAttribute("Count");
            attr.InnerText = "1";
            element.Attributes.Append(attr);
            document.DocumentElement["FavEmojis"].AppendChild(element);

            document.Save("Settings/user-settings.xml");
        }

        private void FavEmojiReplace(XmlDocument document)
        {

        }

        private void FavEmojiPlace(XmlDocument document, XmlElement emoji)
        {
            if (document.DocumentElement.ChildNodes[userId]["Emojis"].ChildNodes.Count < favEmojiCount)
            {
                foreach (XmlNode item in document.DocumentElement.ChildNodes[userId]["Emojis"])
                {
                    //if (item.Attributes[""])
                    //{

                    //}
                }
            }
            else document.DocumentElement.ChildNodes[userId]["Emojis"].AppendChild(emoji);
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