using Avalonia.Markup.Xaml.Templates;
using SecureFolderFS.UI.UserControls.BreadcrumbBar;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    internal sealed class OrderedBreadcrumbBarItemTemplateSelector : GenericTemplateSelector<OrderedBreadcrumbBarItem?>
    {
        public DataTemplate? StandardItemTemplate { get; set; }

        public DataTemplate? LeadingItemTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(OrderedBreadcrumbBarItem? item)
        {
            if (item is null)
                return null;

            return item.IsLeading ? LeadingItemTemplate : StandardItemTemplate;
        }
    }
}
