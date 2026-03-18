using System.Runtime.CompilerServices;
using MauiIcons.Core;
using MauiIcons.Material;
using SecureFolderFS.Core.MobileFS.Platforms.Android;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;
using static SecureFolderFS.Sdk.Constants.DataSources;
using static SecureFolderFS.Sdk.Ftp.Constants;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class AndroidVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystemInfo> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new AndroidFileSystem();
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(IVaultCollectionModel vaultCollectionModel, NewVaultMode mode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var fileExplorerService = DI.Service<IFileExplorerService>();
            yield return new PickerSourceWizardViewModel(DATA_SOURCE_PICKER, fileExplorerService, mode, vaultCollectionModel)
            {
                Icon = new ImageIcon(new MauiIcon() { Icon = MaterialIcons.Storage, IconColor = Colors.White })
            };

            yield return new AccountSourceWizardViewModel(DATA_SOURCE_FTP, "FTP".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResource("source_network_drive_ftp.png") 
            };
            
            yield return new AccountSourceWizardViewModel($"{nameof(SecureFolderFS)}.WebDavClient", "WebDavClient".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResource("source_webdav.png") 
            };

            yield return new AccountSourceWizardViewModel($"{nameof(SecureFolderFS)}.GoogleDrive", "GoogleDrive".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResource("source_gdrive.png") 
            };
            
            yield return new AccountSourceWizardViewModel($"{nameof(SecureFolderFS)}.Dropbox", "Dropbox".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResource("source_dropbox.png") 
            };

            await Task.CompletedTask;
        }
    }
}
