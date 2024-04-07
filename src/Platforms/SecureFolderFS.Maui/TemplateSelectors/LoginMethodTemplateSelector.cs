using SecureFolderFS.Sdk.ViewModels.Views;
using SecureFolderFS.UI.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class LoginMethodTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                PasswordLoginViewModel => PasswordTemplate,
                ErrorViewModel => ErrorTemplate,
                _ => null
            } ?? throw new ArgumentException("Invalid view model type specified.", nameof(item));
        }
    }
}
