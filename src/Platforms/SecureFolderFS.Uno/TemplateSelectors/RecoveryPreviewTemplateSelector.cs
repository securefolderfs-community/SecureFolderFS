using System.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class RecoveryPreviewTemplateSelector : BaseTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? RecoveryTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
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
