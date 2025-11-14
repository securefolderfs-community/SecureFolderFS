using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;
using SecureFolderFS.Storage.Renamable;
using File = Google.Apis.Drive.v3.Data.File;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage
{
    public class GDriveFolder : GDriveStorable, IRenamableFolder, IChildFolder, IGetFirstByName, IGetItem
    {
        public GDriveFolder(DriveService driveService, string id, string name, IFolder? parent = null)
            : base(driveService, id, name, parent)
        {
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string? pageToken = null;

            do
            {
                var request = DriveService.Files.List();
                request.Q = $"'{DetachedId}' in parents and trashed=false";
                request.Fields = "nextPageToken, files(id,name,mimeType)";
                request.PageSize = 100;
                request.PageToken = pageToken;

                var result = await request.ExecuteAsync(cancellationToken);
                pageToken = result.NextPageToken;

                foreach (var file in result.Files)
                {
                    var isFolder = file.MimeType == "application/vnd.google-apps.folder";
                    switch (type)
                    {
                        case StorableType.File when isFolder:
                        case StorableType.Folder when !isFolder:
                            continue;
                    }

                    if (isFolder)
                        yield return new GDriveFolder(DriveService, CombinePaths(Id, file.Id), file.Name, this);
                    else
                        yield return new GDriveFile(DriveService, file.MimeType, CombinePaths(Id, file.Id), file.Name, this);
                }
            } while (pageToken is not null);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            // The id should be in the format "parentId/childId/..."
            // We need to check if this is a direct child of the current folder
            if (!id.StartsWith(Id + '/'))
                throw new FileNotFoundException($"Item with id '{id}' is not a child of folder '{Name}'.");

            // Extract the direct child ID from the path
            var relativePath = id.Substring(Id.Length + 1); // Remove "parentId/"
            var firstSlashIndex = relativePath.IndexOf('/');
            var directChildId = firstSlashIndex >= 0 ? relativePath.Substring(0, firstSlashIndex) : relativePath;

            // Query Google Drive for this specific child
            var request = DriveService.Files.Get(directChildId);
            request.Fields = "id,name,mimeType,parents";

            var file = await request.ExecuteAsync(cancellationToken);

            // Verify the file is actually a child of this folder
            if (file.Parents == null || !file.Parents.Contains(DetachedId))
                throw new FileNotFoundException($"Item with id '{id}' is not a child of folder '{Name}'.");

            // Check if the full path matches (in case we're looking for a nested item)
            var expectedId = CombinePaths(Id, file.Id);
            if (id != expectedId && !id.StartsWith(expectedId + "/"))
                throw new FileNotFoundException($"Item with id '{id}' was not found in folder '{Name}'.");

            var isFolder = file.MimeType == "application/vnd.google-apps.folder";

            if (isFolder)
            {
                var folder = new GDriveFolder(DriveService, CombinePaths(Id, file.Id), file.Name, this);

                // If we need to go deeper into the path, recursively get the item
                if (id != expectedId)
                    return await folder.GetItemAsync(id, cancellationToken);

                return folder;
            }
            else
            {
                // Files can't have children, so the id must match exactly
                if (id != expectedId)
                    throw new FileNotFoundException($"Item with id '{id}' was not found.");

                return new GDriveFile(DriveService, file.MimeType, CombinePaths(Id, file.Id), file.Name, this);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var request = DriveService.Files.List();
            request.Q = $"'{DetachedId}' in parents and name='{name}' and trashed=false";
            request.Fields = "files(id,name,mimeType)";
            request.PageSize = 1;

            var result = await request.ExecuteAsync(cancellationToken);
            if (result.Files.Count == 0)
                throw new FileNotFoundException($"Item with name '{name}' was not found in folder '{Name}'.");

            var file = result.Files[0];
            var isFolder = file.MimeType == "application/vnd.google-apps.folder";

            if (isFolder)
                return new GDriveFolder(DriveService, CombinePaths(Id, file.Id), file.Name, this);
            else
                return new GDriveFile(DriveService, file.MimeType, CombinePaths(Id, file.Id), file.Name, this);
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            // Verify the item belongs to this folder
            var parent = await storable.GetParentAsync(cancellationToken);
            if (parent == null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{storable.Name}' does not belong to folder '{Name}'.");

            // Check if an item with the new name already exists
            var checkRequest = DriveService.Files.List();
            checkRequest.Q = $"'{DetachedId}' in parents and name='{newName}' and trashed=false";
            checkRequest.Fields = "files(id)";
            var checkResult = await checkRequest.ExecuteAsync(cancellationToken);

            if (checkResult.Files.Count > 0)
                throw new IOException($"An item with name '{newName}' already exists in folder '{Name}'.");

            // Perform the rename operation
            var fileMeta = new File()
            {
                Name = newName
            };

            var updateRequest = DriveService.Files.Update(fileMeta, storable.Id);
            updateRequest.Fields = "id,name,mimeType";
            var updatedFile = await updateRequest.ExecuteAsync(cancellationToken);

            // Return a new instance with the updated name
            var isFolder = updatedFile.MimeType == "application/vnd.google-apps.folder";

            if (isFolder)
                return new GDriveFolder(DriveService, storable.Id, updatedFile.Name, this);
            else
                return new GDriveFile(DriveService, updatedFile.MimeType, storable.Id, updatedFile.Name, this);
        }

        /// <inheritdoc/>
        public virtual Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IFolderWatcher>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            // Make sure the item belongs to this folder
            var parent = await item.GetParentAsync(cancellationToken);
            if (parent == null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{item.Name}' does not belong to folder '{Name}'.");

            var request = DriveService.Files.Delete(item.Id);
            await request.ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var request = DriveService.Files.List();
            request.Q = $"'{DetachedId}' in parents and name='{name}' and mimeType='application/vnd.google-apps.folder' and trashed=false";
            request.Fields = "files(id,name,mimeType)";
            var result = await request.ExecuteAsync(cancellationToken);

            File? targetFolder;
            if (result.Files.Count > 0)
            {
                if (!overwrite)
                    throw new IOException($"Folder '{name}' already exists in '{Name}'.");

                targetFolder = result.Files[0];
            }
            else
            {
                var folderMeta = new File()
                {
                    Name = name,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = [ DetachedId ]
                };

                var createReq = DriveService.Files.Create(folderMeta);
                createReq.Fields = "id,name,mimeType";
                targetFolder = await createReq.ExecuteAsync(cancellationToken);
            }

            return new GDriveFolder(DriveService, CombinePaths(Id, targetFolder.Id), targetFolder.Name, this);
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var request = DriveService.Files.List();
            request.Q = $"'{DetachedId}' in parents and name='{name}' and trashed=false";
            request.Fields = "files(id,name,mimeType)";
            var result = await request.ExecuteAsync(cancellationToken);

            File? targetFile;
            if (result.Files.Count > 0)
            {
                if (!overwrite)
                    throw new IOException($"File '{name}' already exists in folder '{Name}'.");

                targetFile = result.Files[0];
            }
            else
            {
                var fileMeta = new File()
                {
                    Name = name,
                    Parents = [ DetachedId ]
                };

                var createReq = DriveService.Files.Create(fileMeta);
                createReq.Fields = "id,name,mimeType";
                targetFile = await createReq.ExecuteAsync(cancellationToken);
            }

            return new GDriveFile(
                DriveService,
                targetFile.MimeType ?? "application/octet-stream",
                CombinePaths(Id, targetFile.Id),
                targetFile.Name,
                this);
        }
    }
}