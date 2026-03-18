using Dropbox.Api;
using Dropbox.Api.Files;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Dropbox.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class DropboxSizeOfProperty : ISizeOfProperty
    {
        private readonly string _id;
        private readonly DropboxClient _client;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public DropboxSizeOfProperty(string id, DropboxClient client)
        {
            _id = id;
            _client = client;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var metadata = await _client.Files.GetMetadataAsync(_id);
            if (metadata is not FileMetadata fileMetadata)
                return null;

            return (long)fileMetadata.Size;
        }
    }
}
