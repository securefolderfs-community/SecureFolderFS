using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class CredentialsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ResetTemplate { get; set; }

        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? SelectionTemplate { get; set; }

        public DataTemplate? ConfirmationTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                LoginViewModel => LoginTemplate,
                CredentialsResetViewModel => ResetTemplate,
                CredentialsSelectionViewModel => SelectionTemplate,
                CredentialsConfirmationViewModel => ConfirmationTemplate,
                _ => null
            };
        }
    }
}
