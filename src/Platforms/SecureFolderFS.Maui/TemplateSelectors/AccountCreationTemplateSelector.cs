using SecureFolderFS.Sdk.Ftp.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class AccountCreationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FtpAccountTemplate { get; set; }
        
        public DataTemplate? GoogleDriveAccountTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                FtpAccountViewModel => FtpAccountTemplate,
                GDriveAccountViewModel => GoogleDriveAccountTemplate,
                _ => null
            };
        }
    }
}
