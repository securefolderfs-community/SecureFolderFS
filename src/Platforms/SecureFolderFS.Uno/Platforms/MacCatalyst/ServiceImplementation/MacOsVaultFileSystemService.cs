using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Core.FUSE.AppModels;
using SecureFolderFS.Core.Routines.Operational;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Helpers;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.Uno.Platforms.Desktop;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class MacOsVaultFileSystemService : BaseVaultFileSystemService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<IFileSystem> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return new FuseFileSystem();
            yield return new MacOsWebDavFileSystem();
        }
    }
}
