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
    public sealed class GDriveFileProperties : ISizeProperties, IDateProperties, IBasicProperties
    {
        private readonly GDriveFile _file;
        private readonly DriveService _driveService;

        public GDriveFileProperties(GDriveFile file, DriveService driveService)
        {
            _file = file;
            _driveService = driveService;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_file.DetachedId);
            request.Fields = "size";
            var file = await request.ExecuteAsync(cancellationToken);
            var sizeProperty = file.Size.HasValue ? new GenericProperty<long>(file.Size.Value) : null;

            return sizeProperty;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_file.DetachedId);
            request.Fields = "createdTime";
            var file = await request.ExecuteAsync(cancellationToken);
            var dateProperty = new GenericProperty<DateTime>(file.CreatedTimeDateTimeOffset?.DateTime ?? DateTime.MinValue);

            return dateProperty;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            var request = _driveService.Files.Get(_file.DetachedId);
            request.Fields = "modifiedTime";
            var file = await request.ExecuteAsync(cancellationToken);
            var dateProperty = new GenericProperty<DateTime>(file.ModifiedTimeDateTimeOffset?.DateTime ?? DateTime.MinValue);

            return dateProperty;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}
