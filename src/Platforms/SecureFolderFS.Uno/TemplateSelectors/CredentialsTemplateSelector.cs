using System.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class CredentialsTemplateSelector : BaseTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? SelectionTemplate { get; set; }

        public DataTemplate? ConfirmationTemaplte { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
        {
            return item switch
            {
                LoginViewModel => LoginTemplate,
                CredentialsSelectionViewModel => SelectionTemplate,
                CredentialsConfirmationViewModel => ConfirmationTemaplte,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
