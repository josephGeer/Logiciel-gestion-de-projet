using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using app_test.Items; 

namespace app_test.Pages
{
    public class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TexteTemplate { get; set; }
        public DataTemplate DessinTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate MediaTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item switch
            {
                Texte => TexteTemplate,
                Dessin => DessinTemplate,
                Items.Image => ImageTemplate,
                Media => MediaTemplate,
                _ => base.SelectTemplateCore(item)
            };
        }
    }
}
