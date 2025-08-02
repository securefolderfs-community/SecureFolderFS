using System.Runtime.CompilerServices;
using MauiIcons.Core;
using MauiIcons.Cupertino;
using SecureFolderFS.Core.MobileFS.Platforms.iOS;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared;
using SecureFolderFS.UI.ServiceImplementation;
using IFileSystem = SecureFolderFS.Storage.VirtualFileSystem.IFileSystem;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class IOSVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new IOSFileSystem();
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(IVaultCollectionModel vaultCollectionModel, NewVaultMode mode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var fileExplorerService = DI.Service<IFileExplorerService>();
            yield return new PickerSourceWizardViewModel(fileExplorerService, mode, vaultCollectionModel)
            {
                Icon = new ImageIcon(new MauiIcon() { Icon = CupertinoIcons.Tray2 })
            };

            yield return new AccountSourceWizardViewModel("FTP".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResourceFile("network_drive_macos.png", false) 
            };
            
            yield return new AccountSourceWizardViewModel("GoogleDrive".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResourceFile("gdrive_icon.png", false) 
            };

            await Task.CompletedTask;
        }
    }
}
