using System.ComponentModel;
using Microsoft.UI.Xaml;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class BreadcrumbItemTemplateSelector : BaseTemplateSelector<INotifyPropertyChanged?>
    {
        public DataTemplate? ParentItemTemplate { get; set; }

        public DataTemplate? LeadingItemTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
        {
            if (item is null)
            {
                return base.SelectTemplateCore(item, container);
            }

            return ParentItemTemplate;
            //return item.IsLeading ? LeadingItemTemplate! : ParentItemTemplate!;
        }
    }
}
