using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage.StorageProperties;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class DavClientSizeOfProperty : ISizeOfProperty
    {
        private readonly string _id;
        private readonly IWebDavClient _client;
        private readonly Uri _baseUri;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public DavClientSizeOfProperty(string id, IWebDavClient client, Uri baseUri)
        {
            _id = id;
            _client = client;
            _baseUri = baseUri;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var uri = new Uri(_baseUri, _id);
            var propfindParams = new PropfindParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await _client.Propfind(uri, propfindParams);

            if (!response.IsSuccessful)
                return null;

            var resource = response.Resources.FirstOrDefault();
            return resource?.ContentLength;
        }
    }
}



