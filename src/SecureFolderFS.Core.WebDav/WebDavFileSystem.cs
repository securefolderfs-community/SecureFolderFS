using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IFileSystem"/>
    public abstract class WebDavFileSystem : IFileSystem
    {
        /// <inheritdoc/>
        public string Id { get; } = Constants.FileSystem.FS_ID;

        /// <inheritdoc/>
        public string Name { get; } = Constants.FileSystem.FS_NAME;

        /// <inheritdoc/>
        public virtual Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            // WebDav should be supported by majority of platforms
            return Task.FromResult(FileSystemAvailability.Available);
        }

        /// <inheritdoc/>
        public virtual async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var webDavOptions = WebDavOptions.ToOptions(options);
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, webDavOptions);
            webDavOptions.SetupValidators(specifics);

            // Check if the port is available
            if (!PortHelpers.IsPortAvailable(webDavOptions.Port))
                webDavOptions.SetPortInternal(PortHelpers.GetNextAvailablePort(webDavOptions.Port));

            var prefix = $"{webDavOptions.Protocol}://{webDavOptions.Domain}:{webDavOptions.Port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            var encryptingDiskStore = new EncryptingDiskStore(specifics.ContentFolder.Id, specifics, !specifics.FileSystemOptions.IsReadOnly);
            var dispatcher = new WebDavDispatcher(new RootDiskStore(specifics.FileSystemOptions.VolumeName, encryptingDiskStore), new RequestHandlerProvider(), null);

            return await MountAsync(
                specifics,
                httpListener,
                webDavOptions,
                dispatcher,
                cancellationToken);
        }

        protected abstract Task<IVFSRoot> MountAsync(
            FileSystemSpecifics specifics,
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken);
    }
}
