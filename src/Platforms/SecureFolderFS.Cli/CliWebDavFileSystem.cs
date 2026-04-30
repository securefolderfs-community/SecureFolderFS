using System.Net;
using NWebDav.Server.Dispatching;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Cli
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    internal sealed class CliWebDavFileSystem : WebDavFileSystem
    {
        /// <inheritdoc/>
        public override Task<string> GetVolumeNameAsync(string candidateName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidateName);
        }

        /// <inheritdoc/>
        protected override async Task<IVfsRoot> MountAsync(FileSystemSpecifics specifics, HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var remotePath = $"{options.Protocol}://{options.Domain}:{options.Port}/";
            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, remotePath);
            webDavWrapper.StartFileSystem();

            var virtualizedRoot = new MemoryFolder(remotePath, options.VolumeName);
            var plaintextRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new WebDavVfsRoot(webDavWrapper, virtualizedRoot, plaintextRoot, specifics);
        }
    }
}
