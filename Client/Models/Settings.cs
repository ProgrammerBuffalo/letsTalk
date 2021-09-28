using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml;

namespace Client.Models
{
    //fav emoji count 
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static Settings instance;
        private string wallaperFolderPath;
        private int messageLoadCount;
        private string selectedWallpaper;
        private Rington selectedUserRington;
        private Rington selectedGroupRington;
        private string selectedLanguage;
        private int chatFontSize;
        private bool showAvatarInGroupMessages;

        private bool canUserRington;
        private bool canGroupRington;
        private bool canUserNotify;
        private bool canGroupNotify;
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
                    instance.wallaperFolderPath = "Settings\\Wallpaper";
                    instance.favEmojiCount = 40;
                    instance.player = new MediaPlayer();
                }
                return instance;
            }
        }

        public string WallaperFolderPath { get => wallaperFolderPath; }

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

        public bool CanUserRington
        {
            get => canUserRington;
            set
            {
                Set(ref canUserRington, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement.ChildNodes[userNodeIndex]["CanUserRington"].InnerText = canUserRington.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public bool CanGroupRington
        {
            get => canGroupRington;
            set
            {
                Set(ref canGroupRington, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement.ChildNodes[userNodeIndex]["CanGroupRington"].InnerText = canGroupRington.ToString();
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

        public string SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                Set(ref selectedLanguage, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement.ChildNodes[userNodeIndex]["SelectedLanguage"].InnerText = selectedLanguage.ToString();
                document.Save("Settings/user-settings.xml");
                ChangeLanguage(selectedLanguage);
            }
        }

        public int ChatFontSize
        {
            get => chatFontSize;
            set
            {
                Set(ref chatFontSize, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement.ChildNodes[userNodeIndex]["ChatFontSize"].InnerText = chatFontSize.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public bool ShowAvatarInGroupMessages
        {
            get => showAvatarInGroupMessages;
            set
            {
                Set(ref showAvatarInGroupMessages, value);
                XmlDocument document = new XmlDocument();
                document.Load("Settings/user-settings.xml");
                document.DocumentElement.ChildNodes[userNodeIndex]["ShowAvatarInGroupMessages"].InnerText = showAvatarInGroupMessages.ToString();
                document.Save("Settings/user-settings.xml");
            }
        }

        public int FavEmojiCount { get => favEmojiCount; }
        public int UserNodeIndex { get => userNodeIndex; }
        public string SettingsPath { get => "Settings/user-settings.xml"; }

        public void LoadSettings(int userId)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            for (int i = 0; i < document.DocumentElement.ChildNodes.Count; i++)
            {
                if (int.Parse(document.DocumentElement.ChildNodes[i].Attributes["UserId"].InnerText) == userId)
                {
                    userNodeIndex = i;

                    selectedLanguage = document.DocumentElement.ChildNodes[i]["SelectedLanguage"].InnerText;
                    messageLoadCount = int.Parse(document.DocumentElement.ChildNodes[i]["MessageLoadCount"].InnerText);

                    selectedWallpaper = document.DocumentElement.ChildNodes[i]["Wallpaper"].InnerText;
                    chatFontSize = int.Parse(document.DocumentElement.ChildNodes[i]["ChatFontSize"].InnerText);
                    showAvatarInGroupMessages = bool.Parse(document.DocumentElement.ChildNodes[i]["ShowAvatarInGroupMessages"].InnerText);

                    canUserRington = bool.Parse(document.DocumentElement.ChildNodes[i]["CanUserRington"].InnerText);
                    canGroupRington = bool.Parse(document.DocumentElement.ChildNodes[i]["CanGroupRington"].InnerText);
                    canUserNotify = bool.Parse(document.DocumentElement.ChildNodes[i]["CanUserNotify"].InnerText);
                    canGroupNotify = bool.Parse(document.DocumentElement.ChildNodes[i]["CanGroupNotify"].InnerText);

                    selectedUserRington = new Rington();
                    selectedUserRington.Name = document.DocumentElement.ChildNodes[i]["UserRington"].Attributes["Name"].InnerText;
                    selectedUserRington.Path = document.DocumentElement.ChildNodes[i]["UserRington"].Attributes["Path"].InnerText;

                    selectedGroupRington = new Rington();
                    selectedGroupRington.Name = document.DocumentElement.ChildNodes[i]["GroupRington"].Attributes["Name"].InnerText;
                    selectedGroupRington.Path = document.DocumentElement.ChildNodes[i]["GroupRington"].Attributes["Path"].InnerText;
                    ChangeLanguage(selectedLanguage);
                    break;
                }
            }
        }

        public void AddUser(int userId)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Settings/user-settings.xml");

            foreach (XmlNode item in document.DocumentElement)
            {
                if (int.Parse(item.Attributes["UserId"].InnerText) == userId) return;
            }

            XmlElement user = document.CreateElement("User");
            XmlAttribute attribute = document.CreateAttribute("UserId");
            attribute.InnerText = userId.ToString();
            user.Attributes.Append(attribute);

            XmlElement element = document.CreateElement("SelectedLanguage");
            element.InnerText = "en";
            user.AppendChild(element);

            element = document.CreateElement("MessageLoadCount");
            element.InnerText = "30";
            user.AppendChild(element);

            element = document.CreateElement("Wallpaper");
            element.InnerText = "/Resources/Wallpapers/skin.jpg";
            user.AppendChild(element);

            element = document.CreateElement("ChatFontSize");
            element.InnerText = "18";
            user.AppendChild(element);

            element = document.CreateElement("ShowAvatarInGroupMessages");
            element.InnerText = "True";
            user.AppendChild(element);

            element = document.CreateElement("CanUserRington");
            element.InnerText = "True";
            user.AppendChild(element);

            element = document.CreateElement("CanGroupRington");
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

            element = document.CreateElement("FavEmojis");
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
                    document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].RemoveChild(document.DocumentElement.ChildNodes[userNodeIndex]["Notifes"].ChildNodes[i]);
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

        //public IEnumerable<Emoji> GetFavEmojis()
        //{
        //    int count;
        //    XmlDocument document = new XmlDocument();
        //    document.Load("Settings/user-settings.xml");

        //    count = document.DocumentElement.ChildNodes[userNodeIndex]["FavEmojis"].ChildNodes.Count;
        //    if (count > favEmojiCount) count = favEmojiCount;
        //    Emoji[] emojis = new Emoji[count];

        //    var node = document.DocumentElement.ChildNodes[userNodeIndex]["FavEmojis"].FirstChild;
        //    for (int i = 0; i < count; i++)
        //    {
        //        emojis[i] = EmojiData.GetEmojiIcon(node.Attributes["Code"].InnerText);
        //        node = node.NextSibling;
        //    }
        //    return emojis;
        //}

        public void DeleteFromDeviceWallpaper()
        {
            foreach (var file in System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + wallaperFolderPath))
                System.IO.File.Delete(file);
        }

        public void ChangeLanguage(string language)
        {
            XmlDocument document = new XmlDocument();
            document.Load("Resources/Languages/" + language + ".xml");
            App.Current.Resources["HelloUser"] = document.DocumentElement["HelloUser"].InnerText;
            App.Current.Resources["HelpUser"] = document.DocumentElement["HelpUser"].InnerText;
            App.Current.Resources["Ok"] = document.DocumentElement["Ok"].InnerText;
            App.Current.Resources["Cancel"] = document.DocumentElement["Cancel"].InnerText;

            App.Current.Resources["Entrance"] = document.DocumentElement["Entrance"].InnerText;
            App.Current.Resources["Name"] = document.DocumentElement["Name"].InnerText;
            App.Current.Resources["Login"] = document.DocumentElement["Login"].InnerText;
            App.Current.Resources["Password"] = document.DocumentElement["Password"].InnerText;
            App.Current.Resources["SignIn"] = document.DocumentElement["SignIn"].InnerText;
            App.Current.Resources["Registrate"] = document.DocumentElement["Registrate"].InnerText;
            App.Current.Resources["Back"] = document.DocumentElement["Back"].InnerText;

            App.Current.Resources["NameErrorLength"] = document.DocumentElement["NameErrorLength"].InnerText;
            App.Current.Resources["NameErrorValidSymbols"] = document.DocumentElement["NameErrorValidSymbols"].InnerText;
            App.Current.Resources["PasswordErrorLength"] = document.DocumentElement["PasswordErrorLength"].InnerText;
            App.Current.Resources["PasswordErrorValidSymbols"] = document.DocumentElement["PasswordErrorValidSymbols"].InnerText;
            App.Current.Resources["AllFieldsError"] = document.DocumentElement["AllFieldsError"].InnerText;

            App.Current.Resources["LoginError"] = document.DocumentElement["LoginError"].InnerText;
            App.Current.Resources["PasswordError"] = document.DocumentElement["PasswordError"].InnerText;
            App.Current.Resources["ConnectionError"] = document.DocumentElement["ConnectionError"].InnerText;
            App.Current.Resources["AuthorizationError"] = document.DocumentElement["AuthorizationError"].InnerText;
            App.Current.Resources["StreamError"] = document.DocumentElement["StreamError"].InnerText;             

            App.Current.Resources["ImageFiles"] = document.DocumentElement["ImageFiles"].InnerText;

            App.Current.Resources["CreateChatroom"] = document.DocumentElement["CreateChatroom"].InnerText;
            App.Current.Resources["Settings"] = document.DocumentElement["Settings"].InnerText;
            App.Current.Resources["AvatarError"] = document.DocumentElement["AvatarError"].InnerText;
            App.Current.Resources["YouAreRemoved"] = document.DocumentElement["YouAreRemoved"].InnerText;
            App.Current.Resources["YouAreAdded"] = document.DocumentElement["YouAreAdded"].InnerText;
            App.Current.Resources["ChatroomError"] = document.DocumentElement["ChatroomError"].InnerText;

            App.Current.Resources["SearchUsers"] = document.DocumentElement["SearchUsers"].InnerText;
            App.Current.Resources["Search"] = document.DocumentElement["Search"].InnerText;
            App.Current.Resources["DragUsersToAdd"] = document.DocumentElement["DragUsersToAdd"].InnerText;
            App.Current.Resources["ShowMore"] = document.DocumentElement["ShowMore"].InnerText;
            App.Current.Resources["EnterNameOfChat"] = document.DocumentElement["EnterNameOfChat"].InnerText;
            App.Current.Resources["MembersOfGroup"] = document.DocumentElement["MembersOfGroup"].InnerText;
            App.Current.Resources["CreateGroup"] = document.DocumentElement["CreateGroup"].InnerText;
            App.Current.Resources["PleaseEnterChatroomName"] = document.DocumentElement["PleaseEnterChatroomName"].InnerText;

            App.Current.Resources["Leave"] = document.DocumentElement["Leave"].InnerText;
            App.Current.Resources["EditChat"] = document.DocumentElement["EditChat"].InnerText;
            App.Current.Resources["Smile"] = document.DocumentElement["Smile"].InnerText;
            App.Current.Resources["File"] = document.DocumentElement["File"].InnerText;
            App.Current.Resources["Save"] = document.DocumentElement["Save"].InnerText;

            App.Current.Resources["GeneralSettings"] = document.DocumentElement["GeneralSettings"].InnerText;
            App.Current.Resources["AppearanceSettings"] = document.DocumentElement["AppearanceSettings"].InnerText;
            App.Current.Resources["NotificationSettings"] = document.DocumentElement["NotificationSettings"].InnerText;

            App.Current.Resources["UserProfile"] = document.DocumentElement["UserProfile"].InnerText;
            App.Current.Resources["Language"] = document.DocumentElement["Language"].InnerText;
            App.Current.Resources["SelectedLanguageText"] = document.DocumentElement["SelectedLanguageText"].InnerText;
            App.Current.Resources["ChatSettings"] = document.DocumentElement["ChatSettings"].InnerText;
            App.Current.Resources["MessageDownloadStep"] = document.DocumentElement["MessageDownloadStep"].InnerText;

            App.Current.Resources["GroupSettings"] = document.DocumentElement["GroupSettings"].InnerText;
            App.Current.Resources["ChatNotification"] = document.DocumentElement["ChatNotification"].InnerText;
            App.Current.Resources["GroupNotification"] = document.DocumentElement["GroupNotification"].InnerText;
            App.Current.Resources["ChatSounds"] = document.DocumentElement["ChatSounds"].InnerText;
            App.Current.Resources["GroupSounds"] = document.DocumentElement["GroupSounds"].InnerText;

            App.Current.Resources["ShowAvatarInGroupMessage"] = document.DocumentElement["ShowAvatarInGroupMessage"].InnerText;
            App.Current.Resources["ChatFontSize"] = document.DocumentElement["ChatFontSize"].InnerText;
            App.Current.Resources["LetsTalkWallpapers"] = document.DocumentElement["LetsTalkWallpapers"].InnerText;
            App.Current.Resources["SelectFromDevice"] = document.DocumentElement["SelectFromDevice"].InnerText;
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