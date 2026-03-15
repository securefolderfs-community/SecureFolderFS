using System;
using System.IO;
using System.Net.Http;
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
        public ICreatedAtProperty CreatedAt => field ??= new DavClientCreatedAtProperty(Id, davClient, baseUri);

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new DavClientLastModifiedAtProperty(Id, davClient, baseUri);

        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => field ??= new DavClientSizeOfProperty(Id, davClient, baseUri);

        public DavClientFile(IWebDavClient davClient, HttpClient httpClient, Uri baseUri, string id, string name, IFolder? parentFolder = null)
            : base(davClient, httpClient, baseUri, id, name, parentFolder)
        {
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            switch (accessMode)
            {
                case FileAccess.Read:
                {
                    return await DavClientReadStream.CreateAsync(httpClient, ResolveUri(Id), cancellationToken);
                }

                case FileAccess.Write or FileAccess.ReadWrite:
                {
                    return await DavClientWriteStream.CreateAsync(httpClient, davClient, ResolveUri(Id), accessMode, cancellationToken);
                }

                default:
                    throw new NotSupportedException($"The {nameof(FileAccess)} '{accessMode}' is not supported on a WebDAV stream.");
            }
        }
    }
}
