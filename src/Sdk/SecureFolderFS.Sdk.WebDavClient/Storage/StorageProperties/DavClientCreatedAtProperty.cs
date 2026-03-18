using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Storage.StorageProperties
{
    /// <inheritdoc cref="ICreatedAtProperty"/>
    public sealed class DavClientCreatedAtProperty : ICreatedAtProperty
    {
        private readonly string _id;
        private readonly IWebDavClient _client;
        private readonly Uri _baseUri;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public DavClientCreatedAtProperty(string id, IWebDavClient client, Uri baseUri)
        {
            _id = id;
            _client = client;
            _baseUri = baseUri;
            Name = nameof(ICreatedAt.CreatedAt);
            Id = $"{id}/{nameof(ICreatedAt.CreatedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var uri = new Uri(_baseUri, _id);
            var response = await _client.Propfind(uri, new() { CancellationToken = cancellationToken });
            if (!response.IsSuccessful)
                return null;

            var resource = response.Resources.FirstOrDefault();
            return resource?.CreationDate;
        }
    }
}
