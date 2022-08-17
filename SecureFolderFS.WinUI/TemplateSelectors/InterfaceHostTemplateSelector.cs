using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.AppHost;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class InterfaceHostTemplateSelector : GenericTemplateSelector<ObservableObject>
    {
        public DataTemplate? MainAppHostDataTemplate { get; set; }

        public DataTemplate? NoVaultsAppHostDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(ObservableObject? item, DependencyObject container)
        {
            if (item is MainAppHostViewModel)
            {
                return MainAppHostDataTemplate;
            }
            else if (item is NoVaultsAppHostViewModel)
            {
                return NoVaultsAppHostDataTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
