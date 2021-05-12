using Client.Models;
using System.Windows;
using System.Windows.Controls;

namespace Client.Utility
{
    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ChatOneDataTemplate { get; set; }
        public DataTemplate ChatGroupDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Chat chat = (Chat)item;
            if (chat is ChatOne) return ChatOneDataTemplate;
            else return ChatGroupDataTemplate;
        }
    }

    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextDataTemplate { get; set; }
        public DataTemplate FileDataTemplate { get; set; }
        public DataTemplate ImageDataTemplate { get; set; }
        public DataTemplate AudioDataTemplate { get; set; }
        public DataTemplate SystemMessageDataTemplate { get; set; }

        // item это тот самый ListViewItem по которому мы выберим один из DateTemplate
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Message message = ((SourceMessage)item).Message;

            if (message is TextMessage) return TextDataTemplate;
            else if (message is FileMessage)
            {
                FileMessage fileMessage = (FileMessage)message;
                string extn = fileMessage.FileName.Substring(fileMessage.FileName.LastIndexOf('.'));
                if (extn == ".png" || extn == ".jpg") return ImageDataTemplate;
                else if (extn == ".mp3" || extn == ".wave") return AudioDataTemplate;
                else return FileDataTemplate;
            }
            else return SystemMessageDataTemplate;
        }
    }

    public class InputMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextDataTemplate { get; set; }
        public DataTemplate FileDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Message message = (Message)item;
            if (message is TextMessage) return TextDataTemplate;
            else return FileDataTemplate;
        }
    }
}
