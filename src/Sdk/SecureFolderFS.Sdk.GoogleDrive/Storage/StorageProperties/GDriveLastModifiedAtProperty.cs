using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage.StorageProperties
{
    /// <inheritdoc cref="ILastModifiedAtProperty"/>
    public sealed class GDriveLastModifiedAtProperty : ILastModifiedAtProperty
    {
        private readonly string _detachedId;
        private readonly DriveService _driveService;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public GDriveLastModifiedAtProperty(string id, string detachedId, DriveService driveService)
        {
            _detachedId = detachedId;
            _driveService = driveService;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_detachedId);
            request.Fields = "modifiedTime";

            var file = await request.ExecuteAsync(cancellationToken);
            if (!file.ModifiedTimeDateTimeOffset.HasValue)
                return null;

            return file.ModifiedTimeDateTimeOffset.Value.DateTime;
        }
    }
}
