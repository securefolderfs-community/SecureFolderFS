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
        private const int MAX_LISTENER_START_ATTEMPTS = 10;

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

            // Start the listener up-front (self-healing onto another port if the bind fails) so a genuine
            // failure surfaces to the caller and fails the unlocking operation, instead of leaving a silently-dead server.
            // Starting here also fixes webDavOptions.Port to the actually bound port before the platform
            // implementation derives the mount URL from it.
            var httpListener = StartListener(webDavOptions);

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

        /// <summary>
        /// Creates and starts an <see cref="HttpListener"/> for the configured WebDAV endpoint, with a fallback to
        /// the next available port if the bind fails (e.g., the port was taken between the availability check and the bind).
        /// </summary>
        /// <param name="options">The WebDAV options. The <see cref="WebDavOptions.Port"/> is updated to the actually bound port.</param>
        /// <returns>A started <see cref="HttpListener"/> bound to the resolved port.</returns>
        private static HttpListener StartListener(WebDavOptions options)
        {
            HttpListenerException? lastException = null;
            for (var attempt = 0; attempt < MAX_LISTENER_START_ATTEMPTS; attempt++)
            {
                var listener = new HttpListener();
                listener.Prefixes.Add($"{options.Protocol}://{options.Domain}:{options.Port}/");
                listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

                try
                {
                    listener.Start();
                    return listener;
                }
                catch (HttpListenerException ex)
                {
                    // The port became unavailable between the check and the bind, proceed with self-heal onto the next free port
                    lastException = ex;
                    listener.Close();
                    options.SetPortInternal(PortHelpers.GetNextAvailablePort(options.Port + 1));
                }
            }

            throw lastException ?? new HttpListenerException();
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
