using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage
{
    public abstract class GDriveStorable : IStorableChild
    {
        /// <summary>
        /// Gets the ID of the object that can exist independently of the parent context.
        /// </summary>
        /// <remarks>
        /// Unline the <see cref="Id"/> which keeps the parent structure, the <see cref="DetachedId"/>
        /// uniquely represents only this storage object.
        /// </remarks>
        public string DetachedId { get; }

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
            DetachedId = Path.GetFileName(Id);
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(ParentFolder);
        }

        /// <summary>
        /// Combines a parent path and a child path into a single path string.
        /// </summary>
        /// <param name="parentPath">The base path that serves as the parent.</param>
        /// <param name="childPath">The relative path of the child to be appended to the parent path.</param>
        /// <returns>A combined string representing the full path.</returns>
        protected static string CombinePaths(string parentPath, string childPath)
        {
            // We specifically don't use System.IO.Path because we want consistent path separators
            return $"{parentPath}/{childPath}";
        }
    }
}