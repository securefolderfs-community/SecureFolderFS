using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class RecoveryPreviewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? LoginTemplate { get; set; }

        public DataTemplate? RecoveryTemplate { get; set; }
        
        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                LoginViewModel => LoginTemplate,
                RecoveryPreviewControlViewModel => RecoveryTemplate,
                _ => null
            };
        }
    }
}
