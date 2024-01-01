using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.UI.Storage.NativeStorage
{
    /// <inheritdoc cref="IFile"/>
    public class NativeFile : NativeStorable<FileInfo>, ILocatableFile, IModifiableFile, IFileExtended, INestedFile
    {
        public NativeFile(FileInfo fileInfo)
            : base(fileInfo)
        {
        }

        public NativeFile(string path)
            : this(new FileInfo(path))
        {
        }

        /// <inheritdoc/>
        public virtual Task<Stream> OpenStreamAsync(FileAccess access, CancellationToken cancellationToken = default)
        {
            return OpenStreamAsync(access, FileShare.None, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<Stream> OpenStreamAsync(FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            var stream = File.Open(Path, FileMode.Open, access, share);
            return Task.FromResult<Stream>(stream);
        }
    }
}
