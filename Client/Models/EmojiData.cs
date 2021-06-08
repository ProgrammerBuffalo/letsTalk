namespace Client.Models
{
    public enum EmojiType { Faces, Hands, Hearts, Nature, Objects, Flags }

    public static class EmojiData
    {
        private static EmojiGroup[] emojiGroups;

        static EmojiData()
        {
            emojiGroups = new EmojiGroup[6];

            Emoji[] emojis = new Emoji[5];
            emojis[0] = new Emoji("happy", "&#001", "Resources/Emojis/faces/001-happy-18.png");
            emojis[1] = new Emoji("cool", "&#002", "Resources/Emojis/faces/002-cool-5.png");
            emojis[2] = new Emoji("happy", "&#003", "Resources/Emojis/faces/003-happy-17.png");
            emojis[3] = new Emoji("surprised", "&#004", "Resources/Emojis/faces/004-surprised-9.png");
            emojis[4] = new Emoji("surprised", "&#005", "Resources/Emojis/faces/004-surprised-9.png");
            emojiGroups[0] = new EmojiGroup("Resources/Emojis/faces/003-happy-17.png", EmojiType.Faces, emojis);

            emojis = new Emoji[5];
            emojis[0] = new Emoji("middle finger", "&#201", "Resources/Emojis/hands/162-middle-finger.png");
            emojis[1] = new Emoji("salute", "&#202", "Resources/Emojis/hands/163-salute.png");
            emojis[2] = new Emoji("strong", "&#203", "Resources/Emojis/hands/164-strong.png");
            emojis[3] = new Emoji("clapping", "&#204", "Resources/Emojis/hands/165-clapping.png");
            emojis[4] = new Emoji("pointing up", "&#205", "Resources/Emojis/hands/166-pointing-up.png");
            emojiGroups[1] = new EmojiGroup("Resources/Emojis/hands/175-hand.png", EmojiType.Hands, emojis);

            emojis = new Emoji[5];
            emojis[0] = new Emoji("kiss", "&#301", "Resources/Emojis/hearts/097-kiss-2.png");
            emojis[1] = new Emoji("email", "&#302", "Resources/Emojis/hearts/098-email.png");
            emojis[2] = new Emoji("cupid", "&#303", "Resources/Emojis/hearts/099-cupid.png");
            emojis[3] = new Emoji("heart", "&#304", "Resources/Emojis/hearts/100-heart-9.png");
            emojis[4] = new Emoji("heart", "&#305", "Resources/Emojis/hearts/101-heart-8.png");
            emojiGroups[2] = new EmojiGroup("Resources/Emojis/hearts/107-heart-4.png", EmojiType.Hearts, emojis);

            emojis = new Emoji[5];
            emojis[0] = new Emoji("plant", "&#401", "Resources/Emojis/nature/001-plant-1.png");
            emojis[1] = new Emoji("koala", "&#402", "Resources/Emojis/nature/002-koala.png");
            emojis[2] = new Emoji("chipmunk", "&#403", "Resources/Emojis/nature/003-chipmunk.png");
            emojis[3] = new Emoji("rat", "&#404", "Resources/Emojis/nature/004-rat.png");
            emojis[4] = new Emoji("mouse", "&#405", "Resources/Emojis/nature/005-mouse-1.png");
            emojiGroups[3] = new EmojiGroup("Resources/Emojis/nature/010-panda.png", EmojiType.Nature, emojis);

            emojis = new Emoji[5];
            emojis[0] = new Emoji("balloon", "&#601", "Resources/Emojis/objects/001-balloon.png");
            emojis[1] = new Emoji("gift", "&#602", "Resources/Emojis/objects/002-gift.png");
            emojis[2] = new Emoji("shopping cart", "&#603", "Resources/Emojis/objects/003-shopping-cart.png");
            emojis[3] = new Emoji("shopping bag", "&#604", "Resources/Emojis/objects/004-shopping-bag.png");
            emojis[4] = new Emoji("frame", "&#605", "Resources/Emojis/objects/005-frame.png");
            emojiGroups[4] = new EmojiGroup("Resources/Emojis/objects/001-balloon.png", EmojiType.Objects, emojis);

            emojis = new Emoji[5];
            emojis[0] = new Emoji("mauritius", "&#801", "Resources/Emojis/flags/001-mauritius.png");
            emojis[1] = new Emoji("cyprus", "&#802", "Resources/Emojis/flags/002-cyprus.png");
            emojis[2] = new Emoji("austria", "&#803", "Resources/Emojis/flags/003-austria.png");
            emojis[3] = new Emoji("oman", "&#804", "Resources/Emojis/flags/004-oman.png");
            emojis[4] = new Emoji("ethiopia", "&#805", "Resources/Emojis/flags/005-ethiopia.png");
            emojiGroups[5] = new EmojiGroup("Resources/Emojis/flags/259-european-union.png", EmojiType.Flags, emojis);
        }

        public static EmojiGroup[] Groups { get => emojiGroups; }

        public static Emoji[] GetEmojiGroup(EmojiType type)
        {
            foreach (var group in emojiGroups)
            {
                if (group.Type == type) return group.Emojis;
            }
            return null;
        }

        public static Emoji GetEmoji(string code)
        {
            foreach (var group in emojiGroups)
            {
                foreach (var emoji in group.Emojis)
                {
                    if (code == emoji.Code) return emoji;
                }
            }
            return null;
        }
    }

    public class EmojiGroup
    {
        private string path;
        EmojiType type;
        private Emoji[] emojis;

        public EmojiGroup()
        {

        }

        public EmojiGroup(string path, EmojiType type, Emoji[] emojis)
        {
            this.path = path;
            this.type = type;
            this.emojis = emojis;
        }

        public string Path { get => path; }
        public EmojiType Type { get => type; }
        public Emoji[] Emojis { get => emojis; }
    }

    public class Emoji
    {
        private string name;
        private string code;
        private string path;

        public Emoji(string name, string code, string path)
        {
            this.name = name;
            this.code = code;
            this.path = path;
        }

        public string Name { get => name; }
        public string Path { get => path; }
        public string Code { get => code; }
    }
}
