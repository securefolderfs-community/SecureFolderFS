using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage
{
    public abstract class GDriveStorable : IStorableChild
    {
        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets the parent folder of the current storable item, if it exists.
        /// </summary>
        protected IFolder? ParentFolder { get; }

        /// <summary>
        /// Provides access to the Google Drive API for managing and interacting with files and folders.
        /// </summary>
        protected DriveService DriveService { get; }

        protected GDriveStorable(DriveService driveService, string id, string name, IFolder? parent = null)
        {
            DriveService = driveService;
            Id = id;
            Name = name;
            ParentFolder = parent;
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(ParentFolder);
        }
    }
}