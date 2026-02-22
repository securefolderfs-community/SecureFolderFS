using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;
using SecureFolderFS.Sdk.GoogleDrive.Storage.StorageProperties;
using SecureFolderFS.Sdk.GoogleDrive.Streams;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage
{
    public class GDriveFile : GDriveStorable, IChildFile
    {
        /// <summary>
        /// Gets the MIME type of the file.
        /// </summary>
        public string MimeType { get; }

        public GDriveFile(DriveService driveService, string mimeType, string id, string name, IFolder? parent = null)
            : base(driveService, id, name, parent)
        {
            MimeType = mimeType;
        }

        /// <inheritdoc/>
        public virtual async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            switch (accessMode)
            {
                case FileAccess.Read:
                {
                    var request = DriveService.Files.Get(DetachedId);
                    request.Fields = "size";
                    var file = await request.ExecuteAsync(cancellationToken);
                    var size = file.Size ?? 0L;

                    // Reuse the request object for the stream to avoid creating another one
                    return new GoogleDriveReadStream(request, size);
                }

                case FileAccess.Write:
                {
                    return new GoogleDriveWriteStream(DriveService, DetachedId, Name, MimeType);
                }

                default:
                    throw new NotSupportedException($"Access mode {accessMode} is not supported.");
            }
        }

        /// <inheritdoc/>
        public override Task<IBasicProperties> GetPropertiesAsync()
        {
            properties ??= new GDriveFileProperties(this, DriveService);
            return Task.FromResult(properties);
        }
    }
}