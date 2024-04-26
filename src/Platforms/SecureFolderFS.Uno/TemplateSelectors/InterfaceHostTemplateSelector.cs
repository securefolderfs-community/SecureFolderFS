using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using System.ComponentModel;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class InterfaceHostTemplateSelector : BaseTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? MainAppHostDataTemplate { get; set; }

        public DataTemplate? NoVaultsAppHostDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
        {
            return item switch
            {
                MainHostViewModel => MainAppHostDataTemplate,
                EmptyHostViewModel => NoVaultsAppHostDataTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
