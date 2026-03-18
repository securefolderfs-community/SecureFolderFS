using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class GDriveSizeOfProperty : ISizeOfProperty
    {
        private readonly string _detachedId;
        private readonly DriveService _driveService;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public GDriveSizeOfProperty(string id, string detachedId, DriveService driveService)
        {
            _detachedId = detachedId;
            _driveService = driveService;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_detachedId);
            request.Fields = "size";

            var file = await request.ExecuteAsync(cancellationToken);
            if (!file.Size.HasValue)
                return null;

            return file.Size.Value;
        }
    }
}
