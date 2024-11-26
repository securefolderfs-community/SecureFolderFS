using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NWebDav.Server;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.Extensions;
using SecureFolderFS.Core.WebDav.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

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

            var webDavOptions = WebDavOptions.ToOptions(options, folder);
            var specifics = FileSystemSpecifics.CreateNew(wrapper.Inner, folder, webDavOptions);

            // Check if the port is available
            if (!PortHelpers.IsPortAvailable(webDavOptions.Port))
                webDavOptions.SetPortInternal(PortHelpers.GetNextAvailablePort(webDavOptions.Port));

            var url = $"{webDavOptions.Protocol}://{webDavOptions.Domain}:{webDavOptions.Port}/";
            var builder = WebApplication.CreateBuilder();
            builder.Services
                .AddNWebDav()
                .AddCipherStore(x =>
                {
                    x.Specifics = specifics;
                });

            var webDavInstance = builder.Build();
            webDavInstance.UseNWebDav();
            _ = webDavInstance.RunAsync(url);

            return await MountAsync(
                webDavOptions,
                webDavInstance,
                cancellationToken);
        }

        protected abstract Task<IVFSRoot> MountAsync(
            WebDavOptions options,
            IAsyncDisposable webDavInstance,
            CancellationToken cancellationToken);
    }
}
