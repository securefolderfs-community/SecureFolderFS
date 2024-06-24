using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class RecoveryPreviewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? RecoveryTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            return item switch
            {
                LoginViewModel => LoginTemplate,
                RecoveryPreviewControlViewModel => RecoveryTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
