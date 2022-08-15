using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.WinUI.Storage.NativeStorage
{
    /// <inheritdoc cref="IFile"/>
    internal sealed class NativeFile : NativeStorable, ILocatableFile, IModifiableFile
    {
        public NativeFile(string path)
            : base(path)
        {
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess access, FileShare share = FileShare.None, CancellationToken cancellationToken = default)
        {
            var stream = File.Open(Path, FileMode.Open, access, share);
            return Task.FromResult<Stream>(stream);
        }
    }
}
