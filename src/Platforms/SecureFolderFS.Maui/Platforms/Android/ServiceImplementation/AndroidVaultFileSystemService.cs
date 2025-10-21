using System.Runtime.CompilerServices;
using MauiIcons.Core;
using MauiIcons.Cupertino;
using SecureFolderFS.Core.MobileFS.Platforms.Android;
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
using static SecureFolderFS.Sdk.Constants.DataSources;
using static SecureFolderFS.Sdk.Ftp.Constants;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class AndroidVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
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
                Icon = new ImageIcon(new MauiIcon() { Icon = CupertinoIcons.Tray2 })
            };

            yield return new AccountSourceWizardViewModel(DATA_SOURCE_FTP, "FTP".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResourceFile("source_network_drive_macos.png", false) 
            };

            yield return new AccountSourceWizardViewModel($"{nameof(SecureFolderFS)}.GoogleDrive", "GoogleDrive".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResourceFile("source_gdrive.png", false) 
            };
            
            yield return new AccountSourceWizardViewModel($"{nameof(SecureFolderFS)}.OneDrive", "OneDrive".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResourceFile("source_onedrive.png", false) 
            };
            
            yield return new AccountSourceWizardViewModel($"{nameof(SecureFolderFS)}.AmazonS3", "AmazonS3".ToLocalized(), mode, vaultCollectionModel)
            {
                Icon = new ImageResourceFile("source_aws_s3.png", false) 
            };

            await Task.CompletedTask;
        }
    }
}
