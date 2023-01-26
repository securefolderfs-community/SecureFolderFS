using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.AppHost;
using System.ComponentModel;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class InterfaceHostTemplateSelector : GenericTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? MainAppHostDataTemplate { get; set; }

        public DataTemplate? NoVaultsAppHostDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
        {
            return item switch
            {
                MainAppHostViewModel => MainAppHostDataTemplate,
                NoVaultsAppHostViewModel => NoVaultsAppHostDataTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
