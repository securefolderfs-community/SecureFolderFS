using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.SystemStorageEx
{
    /// <inheritdoc cref="SystemFile"/>
    public class SystemFileEx : SystemFile
    {
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
        public override Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parent = Directory.GetParent(Path);
            return Task.FromResult<IFolder?>(parent != null ? new SystemFolderEx(parent) : null);
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetRootAsync(CancellationToken cancellationToken = default)
        {
            var root = new DirectoryInfo(Path).Root;
            return Task.FromResult<IFolder?>(new SystemFolderEx(root));
        }
    }
}
