using SecureFolderFS.UI.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class WizardAuthenticationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        public DataTemplate? KeyFileTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                PasswordCreationViewModel => PasswordTemplate,
                KeyFileCreationViewModel => KeyFileTemplate,
                _ => null
            };
        }
    }
}
