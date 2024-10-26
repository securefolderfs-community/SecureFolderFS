using NWebDav.Server;
using NWebDav.Server.Dispatching;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using SecureFolderFS.Core.WebDav.Enums;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
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
        public virtual async Task<IVFSRoot> MountAsync(IFolder folder, IDisposable unlockContract, FileSystemOptions options, CancellationToken cancellationToken = default)
        {
            if (unlockContract is not IWrapper<Security> wrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, options);
            var domain = "localhost";
            var protocol = "http";
            var port = 4949;

            if (options is WebDavOptions webDavOptions)
            {
                port = webDavOptions.PreferredPort;
                domain = webDavOptions.Domain;
                protocol = webDavOptions.Protocol == WebDavProtocolMode.Http ? "http" : "https";
            }

            // Check if the port is available
            if (!PortHelpers.IsPortAvailable(port))
                port = PortHelpers.GetNextAvailablePort(port);

            var prefix = $"{protocol}://{domain}:{port}/";
            var httpListener = new HttpListener();

            httpListener.Prefixes.Add(prefix);
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            // TODO: Implement FileSystemSpecifics
            var cryptoFolder = (IFolder)null!; // new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
            var davFolder = new DavFolder(cryptoFolder);

            // TODO: Remove the following line once the new DavStorage is fully implemented.
            var encryptingDiskStore = new EncryptingDiskStore(specifics.ContentFolder.Id, specifics);
            var dispatcher = new WebDavDispatcher(new RootDiskStore(specifics.FileSystemOptions.VolumeName, encryptingDiskStore), davFolder, new RequestHandlerProvider(), null);

            return await MountAsync(
                port,
                domain,
                protocol,
                httpListener,
                options,
                dispatcher,
                cancellationToken);
        }

        protected abstract Task<IVFSRoot> MountAsync(
            int port,
            string domain,
            string protocol,
            HttpListener listener,
            FileSystemOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken);
    }
}
