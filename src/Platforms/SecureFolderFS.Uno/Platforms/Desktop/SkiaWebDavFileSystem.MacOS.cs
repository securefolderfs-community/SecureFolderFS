using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Uno.Helpers;

namespace SecureFolderFS.Uno.Platforms.Desktop
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    internal sealed partial class SkiaWebDavFileSystem : WebDavFileSystem
    {
#if __UNO_SKIA_MACOS__
        /// <inheritdoc/>
        public override async Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default)
        {
            try
            {
                var folder = new SystemFolderEx("/Volumes");
                var taken = await folder.GetItemsAsync(StorableType.All, cancellationToken).ToArrayAsyncImpl(cancellationToken);

                return CollisionHelpers.GetAvailableName(candidateName, taken.Select(x => x.Name), "{0}_{1}{2}");
            }
            catch (Exception)
            {
                return candidateName;
            }
        }
        
        /// <inheritdoc/>
        protected override async Task<IVfsRoot> MountAsync(
            FileSystemSpecifics specifics,
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, options.Domain, options.Port, options.VolumeName);
            var remoteUri = new Uri(remotePath);

            // Mount WebDAV volume via AppleScript
            Process.Start("/usr/bin/osascript", ["-e", $"mount volume \"{remoteUri.AbsoluteUri}\""]);
            var mountPoint = $"/Volumes/{options.VolumeName}";

            // Create wrapper
            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPoint);
            webDavWrapper.StartFileSystem();

            var virtualizedRoot = new MemoryFolder(mountPoint, options.VolumeName);
            var plaintextRoot = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new WebDavVfsRoot(webDavWrapper, virtualizedRoot, plaintextRoot, specifics);
        }
#endif
    }
}
