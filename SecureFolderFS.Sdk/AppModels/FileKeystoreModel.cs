using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task<IResult<Stream?>> GetKeystoreStreamAsync(FileAccess access = FileAccess.Read, CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;

            _keystoreStream ??= await _keystoreFile.TryOpenStreamAsync(access, FileShare.Read, cancellationToken); // TODO: Use IResult properly
            return new CommonResult<Stream?>(_keystoreStream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keystoreStream?.Dispose();
        }
    }
}
