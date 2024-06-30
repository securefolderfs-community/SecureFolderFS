using SecureFolderFS.UI.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class WizardAuthenticationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PasswordTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                PasswordCreationViewModel => PasswordTemplate,
                _ => null
            } ?? throw new ArgumentException("Invalid view model type specified.", nameof(item));
        }
    }
}
