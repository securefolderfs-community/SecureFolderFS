using Microsoft.UI.Xaml;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class BreadcrumbItemTemplateSelector : BaseTemplateSelector<NavigationItemViewModel>
    {
        public DataTemplate? ParentItemTemplate { get; set; }

        public DataTemplate? LeadingItemTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(NavigationItemViewModel? item, DependencyObject container)
        {
            if (item == null)
            {
                return base.SelectTemplateCore(item, container);
            }

            return item.IsLeading ? LeadingItemTemplate! : ParentItemTemplate!;
        }
    }
}
