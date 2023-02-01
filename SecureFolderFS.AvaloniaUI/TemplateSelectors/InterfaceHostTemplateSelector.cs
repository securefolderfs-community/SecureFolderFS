using System.ComponentModel;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using SecureFolderFS.Sdk.ViewModels.AppHost;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    internal sealed class InterfaceHostTemplateSelector : GenericTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? MainAppHostDataTemplate { get; set; }

        public DataTemplate? NoVaultsAppHostDataTemplate { get; set; }

        protected override IDataTemplate? SelectTemplateCore(INotifyPropertyChanged? item)
        {
            return item switch
            {
                MainAppHostViewModel => MainAppHostDataTemplate,
                NoVaultsAppHostViewModel => NoVaultsAppHostDataTemplate,
                _ => null
            };
        }
    }
}