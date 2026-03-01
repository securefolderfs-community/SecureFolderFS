using Dropbox.Api;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Dropbox.Storage.StorageProperties;
using SecureFolderFS.Sdk.Dropbox.Streams;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Dropbox.Storage
{
    public class DropboxFile : DropboxStorable, IChildFile, ILastModifiedAt, ISizeOf
    {
        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => new DropboxLastModifiedAtProperty(Id, Client);

        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => new DropboxSizeOfProperty(Id, Client);

        public DropboxFile(DropboxClient client, string id, string name, IFolder? parent = null)
            : base(client, id, name, parent)
        {
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            switch (accessMode)
            {
                case FileAccess.Read:
                {
                    var response = await Client.Files.DownloadAsync(Id);
                    var contentStream = await response.GetContentAsStreamAsync();

                    // Wrap so that disposing the stream also disposes the response
                    return new OwningStream(contentStream, response);
                }

                case FileAccess.Write:
                {
                    return new DropboxWriteStream(Client, Id);
                }

                default:
                    throw new NotSupportedException($"Access mode {accessMode} is not supported.");
            }
        }
    }
}