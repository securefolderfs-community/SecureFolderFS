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
using static SecureFolderFS.Sdk.Constants.DataSources;

#if !__UNO_SKIA_MACOS__
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Uno.Platforms.Desktop.ViewModels;
#else
using SecureFolderFS.Core.MacFuse;
#endif

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IVaultFileSystemService"/>
    internal sealed class SkiaVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystemInfo> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new SkiaWebDavFileSystem();

#if __UNO_SKIA_MACOS__
            yield return new MacFuseFileSystem();
#else
            // Inside a Flatpak sandbox the FUSE userspace may appear available, but mounts
            // are confined to the sandbox's mount namespace and invisible to the host
            if (!FuseInstallationViewModel.IsSandboxed)
                yield return new FuseFileSystem();
#endif
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<ItemInstallationViewModel> GetFileSystemInstallationsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
#if !__UNO_SKIA_MACOS__
            // Host packages cannot be installed from inside an application sandbox (e.g., Flatpak)
            if (OperatingSystem.IsLinux() && !FuseInstallationViewModel.IsSandboxed)
            {
                var fuse = new FuseInstallationViewModel();
                await fuse.InitAsync(cancellationToken);
                yield return fuse;
            }
#endif
            await Task.CompletedTask;
            yield break;
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
