using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage.StorageProperties
{
    /// <inheritdoc cref="ICreatedAtProperty"/>
    public sealed class GDriveCreatedAtProperty : ICreatedAtProperty
    {
        private readonly string _detachedId;
        private readonly DriveService _driveService;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public GDriveCreatedAtProperty(string id, string detachedId, DriveService driveService)
        {
            _detachedId = detachedId;
            _driveService = driveService;
            Name = nameof(ICreatedAt.CreatedAt);
            Id = $"{id}/{nameof(ICreatedAt.CreatedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_detachedId);
            request.Fields = "createdTime";

            var file = await request.ExecuteAsync(cancellationToken);
            if (!file.CreatedTimeDateTimeOffset.HasValue)
                return null;

            return file.CreatedTimeDateTimeOffset.Value.DateTime;
        }
    }
}