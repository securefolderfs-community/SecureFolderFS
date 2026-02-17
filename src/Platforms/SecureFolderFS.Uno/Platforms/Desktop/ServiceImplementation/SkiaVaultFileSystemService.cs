using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Shared;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.ViewModels;
using static SecureFolderFS.Sdk.Constants.DataSources;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class SkiaVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new SkiaWebDavFileSystem();

#if !__UNO_SKIA_MACOS__
            yield return new FuseFileSystem();
#endif
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<ItemInstallationViewModel> GetFileSystemInstallationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var dokany = new DokanyInstallationViewModel();
            await dokany.InitAsync(cancellationToken);
            yield return dokany;
            
            var winFsp = new WinFspInstallationViewModel();
            await winFsp.InitAsync(cancellationToken);
            yield return winFsp;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(IVaultCollectionModel vaultCollectionModel, NewVaultMode mode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var fileExplorerService = DI.Service<IFileExplorerService>();
            yield return new PickerSourceWizardViewModel(DATA_SOURCE_PICKER, fileExplorerService, mode, vaultCollectionModel);

            await Task.CompletedTask;
        }
    }
}
