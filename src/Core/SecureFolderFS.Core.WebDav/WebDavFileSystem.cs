using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Storage;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Extensions;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public abstract class WebDavFileSystem : IFileSystemInfo
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
        public virtual async Task<IVfsRoot> MountAsync(IFolder folder, IDisposable unlockContract, IDictionary<string, object> options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var webDavOptions = WebDavOptions.ToOptions(options.AppendContract(unlockContract));
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, webDavOptions);
            webDavOptions.SetupValidators(specifics);

            // Check if the port is available
            if (!PortHelpers.IsPortAvailable(webDavOptions.Port))
                webDavOptions.SetPortInternal(PortHelpers.GetNextAvailablePort(webDavOptions.Port));

            var prefix = $"{webDavOptions.Protocol}://{webDavOptions.Domain}:{webDavOptions.Port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            //var store = new EncryptingDiskStore(specifics.ContentFolder.Id, specifics, !specifics.Options.IsReadOnly);
            var rootFolder = new CryptoFolder(specifics.ContentFolder.Id, specifics.ContentFolder, specifics);
            var store = new BackedDavStore(rootFolder, !specifics.Options.IsReadOnly);
            var dispatcher = new WebDavDispatcher(new RootDiskStore(specifics.Options.VolumeName, store), new RequestHandlerProvider(), null);

            return await MountAsync(
                specifics,
                httpListener,
                webDavOptions,
                dispatcher,
                cancellationToken);
        }

        /// <inheritdoc/>
        public abstract Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mounts the WebDAV file system and initializes the provided HTTP listener, request dispatcher, and file system settings.
        /// </summary>
        /// <param name="specifics">Provides encryption specifics for the file system.</param>
        /// <param name="listener">The HTTP listener to handle WebDAV requests.</param>
        /// <param name="options">The configuration options for the WebDAV file system.</param>
        /// <param name="requestDispatcher">The dispatcher responsible for handling WebDAV requests.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is the initialized virtual file system root.</returns>
        protected abstract Task<IVfsRoot> MountAsync(
            FileSystemSpecifics specifics,
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken);
    }
}
