using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class FileSystemItemTemplateSelector : BaseTemplateSelector<SelectableItemViewModel>
    {
        public DataTemplate? FileSystemTemplate { get; set; }
        
        public DataTemplate? InstallationTemplate { get; set; }
        
        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(SelectableItemViewModel? item, DependencyObject container)
        {
            return item switch
            {
                FileSystemItemViewModel => FileSystemTemplate,
                ItemInstallationViewModel => InstallationTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
