using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IKeystoreModel"/>
    public sealed class FileKeystoreModel : IKeystoreModel
    {
        private readonly IFile _keystoreFile;
        private Stream? _keystoreStream;

        /// <inheritdoc/>
        public IAsyncSerializer<Stream> KeystoreSerializer { get; }

        public FileKeystoreModel(IFile keystoreFile, IAsyncSerializer<Stream> keystoreSerializer)
        {
            _keystoreFile = keystoreFile;
            KeystoreSerializer = keystoreSerializer;
        }

        /// <inheritdoc/>
        public async Task<Stream?> GetKeystoreStreamAsync(CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;

            _keystoreStream ??= await _keystoreFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            return _keystoreStream;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keystoreStream?.Dispose();
        }
    }
}
