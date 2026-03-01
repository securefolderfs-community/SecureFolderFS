using System.Runtime.CompilerServices;
using Dropbox.Api;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Dropbox.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    public sealed class DropboxFolderProperties : IBasicProperties
    {
        private readonly DropboxFolder _folder;
        private readonly DropboxClient _client;

        public DropboxFolderProperties(DropboxFolder folder, DropboxClient client)
        {
            _folder = folder;
            _client = client;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = _folder;
            _ = _client;

            await Task.CompletedTask;
            yield break;
        }
    }
}