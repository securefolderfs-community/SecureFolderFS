using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;

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
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            switch (accessMode)
            {
                case FileAccess.Read:
                {
                    var request = DriveService.Files.Get(DetachedId);
                    var stream = new MemoryStream();

                    await request.DownloadAsync(stream, cancellationToken);
                    stream.Position = 0;

                    return stream;
                }

                case FileAccess.Write:
                {
                    return new GoogleDriveUploadStream(DriveService, Id, Name, MimeType);
                }

                default:
                    throw new NotSupportedException($"Access mode {accessMode} is not supported.");
            }
        }
    }
}