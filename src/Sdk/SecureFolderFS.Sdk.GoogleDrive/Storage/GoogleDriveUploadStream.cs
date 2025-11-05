using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage
{
    /// <summary>
    /// Writable stream that uploads to Google Drive when closed.
    /// Uses resumable upload for large files.
    /// </summary>
    public class GoogleDriveUploadStream : System.IO.MemoryStream
    {
        private readonly DriveService _service;
        private readonly string _fileId;
        private readonly string _name;
        private readonly string _mimeType;

        public GoogleDriveUploadStream(DriveService service, string fileId, string name, string mimeType)
        {
            _service = service;
            _fileId = fileId;
            _name = name;
            _mimeType = mimeType;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Length > 0)
            {
                Position = 0;

                var fileMeta = new File { Name = _name };
                var updateRequest = _service.Files.Update(fileMeta, _fileId, this, _mimeType);

                updateRequest.Fields = "id";
                var prog = updateRequest.Upload();
                _ = prog;
            }

            base.Dispose(disposing);
        }
    }
}