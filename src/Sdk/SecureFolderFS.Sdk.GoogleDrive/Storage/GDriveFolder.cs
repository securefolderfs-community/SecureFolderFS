using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;
using File = Google.Apis.Drive.v3.Data.File;

namespace SecureFolderFS.Sdk.GoogleDrive.Storage
{
    public class GDriveFolder : GDriveStorable, IModifiableFolder, IChildFolder
    {
        public GDriveFolder(DriveService driveService, string id, string name, IFolder? parent = null)
            : base(driveService, id, name, parent)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string? pageToken = null;

            do
            {
                var request = DriveService.Files.List();
                request.Q = $"'{Id}' in parents and trashed=false";
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
                        yield return new GDriveFolder(DriveService, file.Id, file.Name, this);
                    else
                        yield return new GDriveFile(DriveService, file.MimeType, file.Id, file.Name, this);
                }
            } while (pageToken is not null);
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IFolderWatcher>(new NotImplementedException());
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            // Make sure the item belongs to this folder
            var parent = await item.GetParentAsync(cancellationToken);
            if (parent == null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{item.Name}' does not belong to folder '{Name}'.");

            var request = DriveService.Files.Delete(item.Id);
            await request.ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var request = DriveService.Files.List();
            request.Q = $"'{Id}' in parents and name='{name}' and mimeType='application/vnd.google-apps.folder' and trashed=false";
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
                    Parents = [ Id ]
                };

                var createReq = DriveService.Files.Create(folderMeta);
                createReq.Fields = "id,name,mimeType";
                targetFolder = await createReq.ExecuteAsync(cancellationToken);
            }

            return new GDriveFolder(DriveService, targetFolder.Id, targetFolder.Name, this);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var request = DriveService.Files.List();
            request.Q = $"'{Id}' in parents and name='{name}' and trashed=false";
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
                    Parents = [Id]
                };

                var createReq = DriveService.Files.Create(fileMeta);
                createReq.Fields = "id,name,mimeType";
                targetFile = await createReq.ExecuteAsync(cancellationToken);
            }

            return new GDriveFile(
                DriveService,
                targetFile.MimeType ?? "application/octet-stream",
                targetFile.Id,
                targetFile.Name,
                this);
        }
    }
}