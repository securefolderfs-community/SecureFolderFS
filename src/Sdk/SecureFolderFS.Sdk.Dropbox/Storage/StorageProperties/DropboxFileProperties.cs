using System.Runtime.CompilerServices;
using Dropbox.Api;
using Dropbox.Api.Files;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Dropbox.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    public sealed class DropboxFileProperties : ISizeProperties, IDateProperties, IBasicProperties
    {
        private readonly DropboxFile _file;
        private readonly DropboxClient _client;

        public DropboxFileProperties(DropboxFile file, DropboxClient client)
        {
            _file = file;
            _client = client;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            var metadata = await GetFileMetadataAsync();
            return new GenericProperty<long>((long)metadata.Size);
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            // Dropbox has no server-side creation date - ClientModified is the closest
            // equivalent (it reflects the original file date set by the uploading client)
            var metadata = await GetFileMetadataAsync();
            return new GenericProperty<DateTime>(metadata.ClientModified);
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            // ServerModified is the authoritative last-write time recorded by Dropbox
            var metadata = await GetFileMetadataAsync();
            return new GenericProperty<DateTime>(metadata.ServerModified);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
        }

        private async Task<FileMetadata> GetFileMetadataAsync()
        {
            var metadata = await _client.Files.GetMetadataAsync(_file.Id);
            return metadata as FileMetadata ?? throw new InvalidOperationException($"Expected file metadata for '{_file.Name}' but received folder metadata.");
        }
    }
}