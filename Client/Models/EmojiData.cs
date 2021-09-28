using System.Collections.Generic;
using System.Xml;

namespace Client.Models
{
    public enum EmojiType { Faces, Hands, Hearts, Nature, Objects, Flags }

    public static class EmojiData
    {
        //private static EmojiGroup[] emojiGroups;

        private static string emojiPath;

        static EmojiData()
        {
            emojiPath = "Resources/Emojis/emojis.xml";
        }

        //public static EmojiGroup[] Groups { get => emojiGroups; }

        public static EmojiGroup[] GetEmojiGroups()
        {
            XmlDocument documentEmojis = new XmlDocument();
            documentEmojis.Load(emojiPath);

            XmlDocument documentLanguage = new XmlDocument();
            documentLanguage.Load("Resources/Languages/" + Settings.Instance.SelectedLanguage + ".xml");

            EmojiGroup[] groups = new EmojiGroup[documentEmojis.DocumentElement.ChildNodes.Count];
            for (int i = 0; i < documentEmojis.DocumentElement.ChildNodes.Count; i++)
            {
                groups[i] = new EmojiGroup();
                groups[i].Path = documentEmojis.DocumentElement.ChildNodes[i].Attributes["Path"].InnerText;
                groups[i].Code = documentEmojis.DocumentElement.ChildNodes[i].Attributes["Code"].InnerText;
                for (int j = 0; j < documentLanguage.DocumentElement["Emojis"].ChildNodes.Count; j++)
                {
                    if (documentLanguage.DocumentElement["Emojis"].ChildNodes[j].Attributes["Code"].InnerText == groups[i].Code)
                    {
                        groups[i].Type = documentLanguage.DocumentElement["Emojis"].ChildNodes[j].Attributes["Name"].InnerText;
                        break;
                    }
                }
            }
            return groups;
        }

        public static Emoji[] GetEmojiGroup(string codeType)
        {
            XmlDocument documentEmojis = new XmlDocument();
            documentEmojis.Load(emojiPath);

            XmlDocument documentLanguage = new XmlDocument();
            documentLanguage.Load("Resources/Languages/" + Settings.Instance.SelectedLanguage + ".xml");

            Emoji[] emojis = null;

            XmlNode emojiNode = null;
            XmlNode languageNode = null;
            for (int i = 0; i < documentEmojis.DocumentElement.ChildNodes.Count; i++)
            {
                if (documentEmojis.DocumentElement.ChildNodes[i].Attributes["Code"].InnerText == codeType)
                {
                    emojis = new Emoji[documentEmojis.DocumentElement.ChildNodes[i].ChildNodes.Count];
                    emojiNode = documentEmojis.DocumentElement.ChildNodes[i];
                    break;
                }
            }
            for (int i = 0; i < documentLanguage.DocumentElement["Emojis"].ChildNodes.Count; i++)
            {
                if (documentLanguage.DocumentElement["Emojis"].ChildNodes[i].Attributes["Code"].InnerText == codeType)
                {
                    languageNode = documentLanguage.DocumentElement["Emojis"].ChildNodes[i];
                    break;
                }
            }
            for (int i = 0; i < emojiNode.ChildNodes.Count; i++)
            {
                emojis[i] = new Emoji();
                emojis[i].Code = emojiNode.ChildNodes[i].Attributes["Code"].InnerText;
                emojis[i].Path = emojiNode.ChildNodes[i].Attributes["Path"].InnerText;
                for (int j = 0; j < languageNode.ChildNodes.Count; j++)
                {
                    if (emojis[i].Code == languageNode.ChildNodes[j].Attributes["Code"].InnerText)
                    {
                        emojis[i].Name = languageNode.ChildNodes[j].Attributes["Name"].InnerText;
                        break;
                    }
                }
            }
            return emojis;
        }

        public static Emoji GetEmojiIcon(string code)
        {
            XmlDocument documentEmojis = new XmlDocument();
            documentEmojis.Load(emojiPath);
            for (int i = 0; i < documentEmojis.DocumentElement.ChildNodes.Count; i++)
            {
                for (int j = 0; j < documentEmojis.DocumentElement.ChildNodes[i].ChildNodes.Count; j++)
                {
                    if (documentEmojis.DocumentElement.ChildNodes[i].ChildNodes[j].Attributes["Code"].InnerText == code)
                    {
                        Emoji emoji = new Emoji();
                        emoji.Code = code;
                        emoji.Path = documentEmojis.DocumentElement.ChildNodes[i].ChildNodes[j].Attributes["Path"].InnerText;
                        return emoji;
                    }
                }
            }
            return null;
        }

        public static Emoji[] GetEmojis(string[] codes)
        {
            XmlDocument emojiDocument = new XmlDocument();
            emojiDocument.Load(emojiPath);

            XmlDocument languageDocumnet = new XmlDocument();
            languageDocumnet.Load("Resources/Languages/" + Settings.Instance.SelectedLanguage + ".xml");

            Emoji[] emojis = new Emoji[codes.Length];

            for (int i = 0; i < codes.Length; i++)
            {
                emojis[i] = new Emoji();
                emojis[i].Code = codes[i];
                SetEmojiProperty(emojiDocument.DocumentElement, emojis[i], "Path");
                SetEmojiProperty(languageDocumnet.DocumentElement["Emojis"], emojis[i], "Name");
            }
            return emojis;
        }

        public static void SetEmojiProperty(XmlNode node, Emoji emoji, string property)
        {
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
                {
                    if (node.ChildNodes[i].ChildNodes[j].Attributes["Code"].InnerText == emoji.Code)
                    {
                        emoji.GetType().GetProperty(property).SetValue(emoji, node.ChildNodes[i].ChildNodes[j].Attributes[property].InnerText);
                        return;
                    }
                }
            }
        }

        public static Emoji GetEmoji(XmlDocument document, string code)
        {
            for (int j = 0; j < document.DocumentElement.ChildNodes.Count; j++)
            {
                for (int k = 0; k < document.DocumentElement.ChildNodes[j].ChildNodes.Count; k++)
                {
                    if (document.DocumentElement.ChildNodes[j].ChildNodes[k].Attributes["Code"].InnerText == code)
                    {
                        Emoji emoji = new Emoji();
                        emoji.Code = code;
                        emoji.Path = document.DocumentElement.ChildNodes[j].ChildNodes[k].Attributes["Path"].InnerText;
                        return emoji;
                    }
                }
            }
            return null;
        }

        public static Emoji[] GetFavEmojis()
        {
            int count;
            XmlDocument document = new XmlDocument();
            document.Load(Settings.Instance.SettingsPath);

            count = document.DocumentElement.ChildNodes[Settings.Instance.UserNodeIndex]["FavEmojis"].ChildNodes.Count;
            if (count > Settings.Instance.FavEmojiCount) count = Settings.Instance.FavEmojiCount;
            Emoji[] emojis = new Emoji[count];

            var node = document.DocumentElement.ChildNodes[Settings.Instance.UserNodeIndex]["FavEmojis"].FirstChild;
            string[] codes = new string[count];

            for (int i = 0; i < count; i++)
            {
                codes[i] = node.Attributes["Code"].InnerText;
                node = node.NextSibling;
            }
            return GetEmojis(codes);
        }

        public static void AddFavEmoji(string code)
        {
            XmlDocument document = new XmlDocument();
            document.Load(Settings.Instance.SettingsPath);

            var node = document.DocumentElement.ChildNodes[Settings.Instance.UserNodeIndex]["FavEmojis"].FirstChild;
            for (int i = 0; i < document.DocumentElement.ChildNodes[Settings.Instance.UserNodeIndex]["FavEmojis"].ChildNodes.Count; i++)
            {
                if (node.Attributes["Code"].InnerText == code)
                {
                    var temp = node;
                    node.Attributes["Count"].InnerText = (int.Parse(node.Attributes["Count"].InnerText) + 1).ToString();
                    for (int j = i - 1; j >= 0; j--)
                    {
                        node = node.PreviousSibling;
                        if (int.Parse(node.Attributes["Count"].InnerText) < int.Parse(temp.Attributes["Count"].InnerText))
                        {
                            document.DocumentElement.ChildNodes[Settings.Instance.UserNodeIndex]["FavEmojis"].InsertBefore(temp, node);
                            break;
                        }
                    }
                    document.Save(Settings.Instance.SettingsPath);
                    return;
                }
                node = node.NextSibling;
            }
            XmlElement element = document.CreateElement("Emoji");

            XmlAttribute attribute = document.CreateAttribute("Code");
            attribute.InnerText = code;
            element.Attributes.Append(attribute);

            attribute = document.CreateAttribute("Count");
            attribute.InnerText = "1";
            element.Attributes.Append(attribute);

            document.DocumentElement.ChildNodes[Settings.Instance.UserNodeIndex]["FavEmojis"].AppendChild(element);
            document.Save(Settings.Instance.SettingsPath);
        }

        public static LinkedList<Emoji> GetEmojiByName(string name)
        {
            LinkedList<Emoji> emojis = new LinkedList<Emoji>();
            LinkedList<string> names = new LinkedList<string>();

            XmlDocument emojiDocument = new XmlDocument();
            emojiDocument.Load(emojiPath);

            XmlDocument languageDocumnet = new XmlDocument();
            languageDocumnet.Load("Resources/Languages/" + Settings.Instance.SelectedLanguage + ".xml");

            for (int i = 0; i < languageDocumnet.DocumentElement["Emojis"].ChildNodes.Count; i++)
            {
                for (int j = 0; j < languageDocumnet.DocumentElement["Emojis"].ChildNodes[i].ChildNodes.Count; j++)
                {
                    if (Utility.StringExtensions.ContainsAtStart(languageDocumnet.DocumentElement["Emojis"].ChildNodes[i].ChildNodes[j].Attributes["Name"].InnerText, name))
                    {
                        emojis.AddLast(new Emoji());
                        emojis.Last.Value.Code = languageDocumnet.DocumentElement["Emojis"].ChildNodes[i].ChildNodes[j].Attributes["Code"].InnerText;
                        emojis.Last.Value.Name = languageDocumnet.DocumentElement["Emojis"].ChildNodes[i].ChildNodes[j].Attributes["Name"].InnerText;
                    }
                }
            }

            var node = emojis.First;
            for (int i = 0; i < emojis.Count; i++)
            {
                SetEmojiProperty(emojiDocument.DocumentElement, node.Value, "Path");
                node = node.Next;
            }
            return emojis;
        }
    }

    public class EmojiGroup
    {
        private string path;
        private string type;
        private string code;
        private Emoji[] emojis;

        public EmojiGroup()
        {

        }

        public EmojiGroup(string path, string type, Emoji[] emojis)
        {
            this.path = path;
            this.type = type;
            this.emojis = emojis;
        }

        public string Path { get => path; set => path = value; }
        public string Type { get => type; set => type = value; }
        public string Code { get => code; set => code = value; }
        public Emoji[] Emojis { get => emojis; set => emojis = value; }
    }

    public class Emoji
    {
        private string name;
        private string code;
        private string path;

        public Emoji()
        {

        }

        public Emoji(string name, string code, string path)
        {
            this.name = name;
            this.code = code;
            this.path = path;
        }

        public string Name { get => name; set => name = value; }
        public string Path { get => path; set => path = value; }
        public string Code { get => code; set => code = value; }
    }
}
