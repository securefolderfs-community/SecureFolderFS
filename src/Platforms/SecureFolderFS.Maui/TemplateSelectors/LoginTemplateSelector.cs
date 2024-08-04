using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class LoginTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        public DataTemplate? ErrorTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                PasswordLoginViewModel => PasswordTemplate,
                KeyFileLoginViewModel => KeyFileTemplate,
                ErrorViewModel => ErrorTemplate,
                _ => null
            };
        }
    }
}
