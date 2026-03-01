using Dropbox.Api;
using Dropbox.Api.Files;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.Dropbox.Storage
{
    public class DropboxFile : DropboxStorable, IChildFile
    {
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
                    var stream = new MemoryStream();
                    await (await response.GetContentAsStreamAsync()).CopyToAsync(stream, cancellationToken);
                    stream.Position = 0;
                    return stream;
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

    /// <summary>
    /// A write-only <see cref="MemoryStream"/> that uploads its buffered contents to Dropbox
    /// on <see cref="Dispose"/>, overwriting the existing file.
    /// Mirrors the GoogleDriveWriteStream pattern.
    /// </summary>
    internal sealed class DropboxWriteStream : MemoryStream
    {
        private readonly DropboxClient _client;
        private readonly string _path;
        private bool _disposed;

        public DropboxWriteStream(DropboxClient client, string path)
        {
            _client = client;
            _path = path;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                Position = 0;

                _client.Files.UploadAsync(
                        _path,
                        WriteMode.Overwrite.Instance,
                        body: this)
                    .GetAwaiter()
                    .GetResult();
            }

            base.Dispose(disposing);
        }
    }
}