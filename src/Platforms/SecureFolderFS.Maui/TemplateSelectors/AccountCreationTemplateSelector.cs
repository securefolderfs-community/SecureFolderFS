using SecureFolderFS.Sdk.Ftp.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class AccountCreationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FtpAccountTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                FtpAccountViewModel => FtpAccountTemplate,
                _ => null
            };
        }
    }
}
