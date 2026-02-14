using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.Storage.SystemStorageEx.StorageProperties;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage.FileShareOptions;

namespace SecureFolderFS.Storage.SystemStorageEx
{
    /// <inheritdoc cref="SystemFile"/>
    public class SystemFileEx : SystemFile, IFileOpenShare, IStorableProperties
    {
        protected IBasicProperties? properties;

        /// <inheritdoc/>
        public SystemFileEx(string path)
            : base(path)
        {
        }

        /// <inheritdoc/>
        public SystemFileEx(FileInfo info)
            : base(info)
        {
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, FileShare shareMode, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return Info.Open(new FileStreamOptions()
            {
                Access = accessMode,
                Share = shareMode,
                Options = FileOptions.Asynchronous
            });
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parent = Directory.GetParent(Path);
            return Task.FromResult<IFolder?>(parent != null ? new SystemFolderEx(parent) : null);
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetRootAsync(CancellationToken cancellationToken = default)
        {
            var root = Info.Directory?.Root;
            if (root is null)
                return Task.FromResult<IFolder?>(null);

            return Task.FromResult<IFolder?>(new SystemFolderEx(root));
        }

        /// <inheritdoc/>
        public Task<IBasicProperties> GetPropertiesAsync()
        {
            properties ??= new SystemFileExProperties(Info);
            return Task.FromResult(properties);
        }
    }
}
