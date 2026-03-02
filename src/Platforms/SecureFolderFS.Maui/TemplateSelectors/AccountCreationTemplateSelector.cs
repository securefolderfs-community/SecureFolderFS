using SecureFolderFS.Sdk.Dropbox.ViewModels;
using SecureFolderFS.Sdk.Ftp.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.ViewModels;
using SecureFolderFS.Sdk.WebDavClient.ViewModels;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class AccountCreationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FtpAccountTemplate { get; set; }

        public DataTemplate? WebDavClientAccountTemplate { get; set; }

        public DataTemplate? GoogleDriveAccountTemplate { get; set; }

        public DataTemplate? DropboxAccountTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                FtpAccountViewModel => FtpAccountTemplate,
                WebDavClientAccountViewModel => WebDavClientAccountTemplate,
                GDriveAccountViewModel => GoogleDriveAccountTemplate,
                DropboxAccountViewModel => DropboxAccountTemplate,
                _ => null
            };
        }
    }
}
