using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.AppModels;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IRecycleBinService"/>
    public class RecycleBinService : IRecycleBinService
    {
        /// <inheritdoc/>
        public async Task ConfigureRecycleBinAsync(IVFSRoot vfsRoot, long maxSize, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            // TODO: Update configuration data model with appropriate IsRecycleBinEnabled value
            vfsRoot.Options.DangerousSetRecycleBin(maxSize);

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<IRecycleBinFolder> GetRecycleBinAsync(IVFSRoot vfsRoot, CancellationToken cancellationToken = default)
        {
            if (vfsRoot is not IWrapper<FileSystemSpecifics> specificsWrapper)
                throw new ArgumentException($"The specified {nameof(IVFSRoot)} instance is not supported.");

            var recycleBin = await AbstractRecycleBinHelpers.GetOrCreateRecycleBinAsync(specificsWrapper.Inner, cancellationToken);
            if (recycleBin is not IModifiableFolder modifiableRecycleBin)
                throw new UnauthorizedAccessException("Could not retrieve the recycle bin folder.");

            return new VaultRecycleBin(modifiableRecycleBin, vfsRoot, specificsWrapper.Inner);
        }
    }
}
