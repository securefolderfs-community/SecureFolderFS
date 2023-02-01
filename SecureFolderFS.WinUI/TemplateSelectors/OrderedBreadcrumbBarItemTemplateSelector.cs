using Microsoft.UI.Xaml;
using SecureFolderFS.UI.UserControls.BreadcrumbBar;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class OrderedBreadcrumbBarItemTemplateSelector : GenericTemplateSelector<OrderedBreadcrumbBarItem?>
    {
        public DataTemplate? StandardItemTemplate { get; set; }

        public DataTemplate? LeadingItemTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(OrderedBreadcrumbBarItem? item, DependencyObject container)
        {
            if (item is null)
                return base.SelectTemplateCore(item, container);

            return item.IsLeading ? LeadingItemTemplate : StandardItemTemplate;
        }
    }
}
