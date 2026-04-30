using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Cli;

internal sealed class CliVaultFileSystemService : BaseVaultFileSystemService
{
    public override async IAsyncEnumerable<IFileSystemInfo> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        // Keep ordering aligned with desktop targets: WebDAV first, then native adapters.
        yield return new CliWebDavFileSystem();
        yield return new FuseFileSystem();

#if SFFS_WINDOWS_FS
        yield return new SecureFolderFS.Core.WinFsp.WinFspFileSystem();
        yield return new SecureFolderFS.Core.Dokany.DokanyFileSystem();
#endif
    }

    public override async IAsyncEnumerable<BaseDataSourceWizardViewModel> GetSourcesAsync(IVaultCollectionModel vaultCollectionModel, NewVaultMode mode,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = vaultCollectionModel;
        _ = mode;
        await Task.CompletedTask;
        yield break;
    }
}

