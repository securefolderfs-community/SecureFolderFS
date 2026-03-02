using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.WebDavClient.Storage.StorageProperties;
using SecureFolderFS.Sdk.WebDavClient.Streams;
using SecureFolderFS.Storage.StorageProperties;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Storage
{
    public class DavClientFile : DavClientStorable, IChildFile, ICreatedAt, ILastModifiedAt, ISizeOf
    {
        /// <inheritdoc/>
        public ICreatedAtProperty CreatedAt => field ??= new DavClientCreatedAtProperty(Id, client, baseUri);

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new DavClientLastModifiedAtProperty(Id, client, baseUri);

        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => field ??= new DavClientSizeOfProperty(Id, client, baseUri);

        public DavClientFile(IWebDavClient client, Uri baseUri, string id, string name, IFolder? parentFolder = null)
            : base(client, baseUri, id, name, parentFolder)
        {
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            switch (accessMode)
            {
                case FileAccess.Read:
                {
                    var getParams = new GetFileParameters
                    {
                        CancellationToken = cancellationToken
                    };
                    var response = await client.GetRawFile(ResolveUri(Id), getParams);
                    if (!response.IsSuccessful)
                        throw new IOException($"Failed to download file '{Name}': {response.StatusCode}");

                    return response.Stream;
                }

                case FileAccess.Write:
                {
                    return new DavClientWriteStream(client, baseUri, Id);
                }

                default:
                    throw new NotSupportedException($"The {nameof(FileAccess)} '{accessMode}' is not supported on a WebDAV stream.");
            }
        }
    }
}
