using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class BreadcrumbItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ParentItemTemplate { get; set; }

        public DataTemplate LeadingItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is not NavigationItemViewModel itemViewModel)
            {
                return base.SelectTemplateCore(item, container);
            }

            return itemViewModel.IsLeading ? LeadingItemTemplate : ParentItemTemplate;
        }
    }
}
