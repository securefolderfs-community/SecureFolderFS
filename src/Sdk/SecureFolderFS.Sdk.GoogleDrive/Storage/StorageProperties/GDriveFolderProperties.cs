using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    public sealed class GDriveFolderProperties : IDateProperties, IBasicProperties
    {
        private readonly GDriveFolder _folder;
        private readonly DriveService _driveService;

        public GDriveFolderProperties(GDriveFolder folder, DriveService driveService)
        {
            _folder = folder;
            _driveService = driveService;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_folder.DetachedId);
            request.Fields = "createdTime";
            var file = await request.ExecuteAsync(cancellationToken);
            var dateProperty = new GenericProperty<DateTime>(file.CreatedTimeDateTimeOffset?.DateTime ?? DateTime.MinValue);

            return dateProperty;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_folder.DetachedId);
            request.Fields = "modifiedTime";
            var file = await request.ExecuteAsync(cancellationToken);
            var dateProperty = new GenericProperty<DateTime>(file.ModifiedTimeDateTimeOffset?.DateTime ?? DateTime.MinValue);

            return dateProperty;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}

