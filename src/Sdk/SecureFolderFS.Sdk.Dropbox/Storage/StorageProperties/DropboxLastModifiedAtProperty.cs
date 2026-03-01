using Dropbox.Api;
using Dropbox.Api.Files;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Dropbox.Storage.StorageProperties
{
    /// <inheritdoc cref="ILastModifiedAtProperty"/>
    public sealed class DropboxLastModifiedAtProperty : ILastModifiedAtProperty
    {
        private readonly string _id;
        private readonly DropboxClient _client;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public DropboxLastModifiedAtProperty(string id, DropboxClient client)
        {
            _id = id;
            _client = client;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var metadata = await _client.Files.GetMetadataAsync(_id);
            if (metadata is not FileMetadata fileMetadata)
                return null;

            return fileMetadata.ClientModified;
        }
    }
}
