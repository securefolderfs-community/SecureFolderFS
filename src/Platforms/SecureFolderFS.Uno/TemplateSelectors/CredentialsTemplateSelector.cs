using System.ComponentModel;
using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class CredentialsTemplateSelector : BaseTemplateSelector<INotifyPropertyChanged>
    {
        public DataTemplate? ResetTemplate { get; set; }

        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? SelectionTemplate { get; set; }

        public DataTemplate? ConfirmationTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(INotifyPropertyChanged? item, DependencyObject container)
        {
            return item switch
            {
                LoginViewModel => LoginTemplate,
                CredentialsResetViewModel => ResetTemplate,
                CredentialsSelectionViewModel => SelectionTemplate,
                CredentialsConfirmationViewModel => ConfirmationTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
