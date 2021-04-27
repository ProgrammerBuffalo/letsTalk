using Client.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Client.Utility
{
    // это класс который будет определять какой из DataTemplate применить
    public class MessageTemplateSelector : DataTemplateSelector
    {
        //private static string Text;
        //private static string Image;

        //static MessageTemplateSelector()
        //{
        //    Text = @"<Border Background=""Red""> <TextBlock Text=""asdasd sda sd"" FontSize=""12""></TextBlock> </Border>";
        //    Image = @"<Border Background=""Green""> <Image Source=""{Binding Path=Message.Path}""></Image> </Border>";

        //    TextDataTemlpate = (DataTemplate)dictionary["TextDataTemplate"];
        //    ImageDataTemplate = (DataTemplate)dictionary["ImageDataTemplate"];
        //    FileDataTemaplte = (DataTemplate)dictionary["FileDataTemplate"];
        //    AudioDataTemplate = (DataTemplate)dictionary["AudioDataTemplate"];
        //}

        public MessageTemplateSelector()
        {
            //ResourceDictionary dictionary = new ResourceDictionary();
            //dictionary.Source = new System.Uri("/WindowResources/MessageDicitionary.xaml", System.UriKind.Relative);
            //SourceDT = (DataTemplate)dictionary["SourceMessageDateTemplate"];
            //UserDT = (DataTemplate)dictionary["UserMessageDataTemplate"];
        }

        //public DataTemplate SourceMessageDateTemplate { get; set; }
        //public DataTemplate UserMessageDataTemplate { get; set; }
        //public DataTemplate GroupMessageDataTemplate { get; set; }
        //public DataTemplate SystemMessageDataTemplate { get; set; }

        public DataTemplate TextDataTemplate { get; set; }
        public DataTemplate FileDataTemplate { get; set; }
        public DataTemplate ImageDataTemplate { get; set; }
        public DataTemplate AudioDataTemplate { get; set; }

        // item это тот самый ListViewItem по которому мы выберим один из DateTemplate
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Message message = ((SourceMessage)item).Message;

            if (message is TextMessage) return TextDataTemplate;
            else
            {
                FileMessage fileMessage = (FileMessage)message;
                string extn = fileMessage.FileName.Substring(fileMessage.FileName.LastIndexOf('.'));
                if (extn == ".png" || extn == ".jpg") return ImageDataTemplate;
                else if (extn == ".mp3" || extn == ".wave") return AudioDataTemplate;
                else return FileDataTemplate;
            }
            //DataTemplate select;

            //Border border = (Border)select.LoadContent();

            //ContentPresenter cp = (ContentPresenter)border.Child;
            //ParserContext parserContext = new ParserContext();
            //parserContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            //cp.Content = XamlReader.Parse(Text, parserContext);
            //border.Child = (UIElement)XamlReader.Parse(Text, parserContext);
            //border.Background = Brushes.Aqua;
            //ContentPresenter element = (ContentPresenter)container;
            //element.Content = TextDataTemaplte;

            //var a = select.VisualTree.FirstChild.FirstChild;
            //var b = TextDT.VisualTree.FirstChild;
            //a.AppendChild(b);
        }
    }
}

//DataType = "{x:Type local:ClassOne}"

//var dataTemplateString =
//    @"<DataTemplate>" +
//        @"<StackPanel>" +
//            @"<telerik:RadButton Content=""Button Content"" />" +
//            @"<TextBlock Text=""{Binding }"" />" +
//            @"<local:MyCustomControl />" +
//        @"</StackPanel>" +
//    @"</DataTemplate>";

//ParserContext parserContext = new ParserContext();

//// This namespace is required for all the default elements that don't require a prefix, like DataTemplate, TextBlock, StackPanel, etc. 
//parserContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
//// This is the telerik namespace required by the RadButton in this case. 
//parserContext.XmlnsDictionary.Add("telerik", "http://schemas.telerik.com/2008/xaml/presentation");
//// This is a local namespace pointing to a namespace from your application. The same approach is used for any other namespaces. 
//parserContext.XmlnsDictionary.Add("local", "clr-namespace:MyNamespace;assembly=MyAssemblyName");

//DataTemplate template = (DataTemplate)XamlReader.Parse(dataTemplateString, parserContext);