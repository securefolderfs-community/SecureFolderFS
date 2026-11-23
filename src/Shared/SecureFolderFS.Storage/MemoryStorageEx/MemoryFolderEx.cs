using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.Streams;

namespace SecureFolderFS.Storage.MemoryStorageEx
{
    /// <inheritdoc cref="MemoryFolder"/>
    public class MemoryFolderEx : MemoryFolder, IRenamableFolder
    {
        private readonly IStreamSource? _streamSource;

        /// <inheritdoc/>
        public MemoryFolderEx(string id, string name, MemoryFolder? parent, IStreamSource? streamSource = null)
            : base(id, name)
        {
            _streamSource = streamSource;
            Parent = parent;
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            var oldPath = storable.Id;
            var newPath = Path.Combine(Id, newName);

            await Task.CompletedTask;
            switch (storable)
            {
                case MemoryFileEx memoryFile:
                {
                    FolderContents.Remove(oldPath);
                    var newFile = new MemoryFileEx(newPath, newName, memoryFile.InternalStream, this, _streamSource);
                    newFile.SetParent(this);
                    FolderContents.Add(newPath, newFile);

                    return newFile;
                }

                case MemoryFolderEx memoryFolder:
                {
                    FolderContents.Remove(oldPath);
                    var newFolder = new MemoryFolderEx(newPath, newName, this, _streamSource);
                    newFolder.SetParent(this);

                    // A rename has to carry the subtree with it. Identifiers are derived from the parent's
                    // path, so every descendant is re-created underneath the renamed folder
                    await newFolder.AdoptContentsFromAsync(memoryFolder, cancellationToken);
                    FolderContents.Add(newPath, newFolder);

                    return newFolder;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(storable));
            }
        }

        private async Task AdoptContentsFromAsync(MemoryFolderEx source, CancellationToken cancellationToken)
        {
            await foreach (var item in source.GetItemsAsync(StorableType.All, cancellationToken))
            {
                switch (item)
                {
                    case MemoryFileEx memoryFile:
                    {
                        // The stream instance is shared rather than copied, so the contents move with the item
                        var adoptedFile = new MemoryFileEx(Path.Combine(Id, memoryFile.Name), memoryFile.Name, memoryFile.InternalStream, this, _streamSource);
                        adoptedFile.SetParent(this);
                        FolderContents[adoptedFile.Id] = adoptedFile;

                        break;
                    }

                    case MemoryFolderEx memoryFolder:
                    {
                        var adoptedFolder = new MemoryFolderEx(Path.Combine(Id, memoryFolder.Name), memoryFolder.Name, this, _streamSource);
                        adoptedFolder.SetParent(this);

                        await adoptedFolder.AdoptContentsFromAsync(memoryFolder, cancellationToken);
                        FolderContents[adoptedFolder.Id] = adoptedFolder;

                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingFolderKvp = FolderContents.FirstOrDefault(x => x.Value.Name == name && x.Value is IFolder);
            var existingFolder = existingFolderKvp.Value as IChildFolder;

            if (overwrite && existingFolder is not null)
                await DeleteAsync(existingFolder, cancellationToken);

            var emptyMemoryFolder = new MemoryFolderEx(Path.Combine(Id, name), name, this, _streamSource);
            emptyMemoryFolder.SetParent(this);

            var folder = overwrite ? emptyMemoryFolder : (existingFolder ?? emptyMemoryFolder);
            if (!FolderContents.TryAdd(folder.Id, folder))
                FolderContents[folder.Id] = folder;

            return folder;
        }

        /// <inheritdoc/>
        public override async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingFileKvp = FolderContents.FirstOrDefault(x => x.Value.Name == name);
            var existingFile = (IChildFile?)existingFileKvp.Value;

            if (overwrite && existingFile is not null)
                await DeleteAsync(existingFile, cancellationToken);

            var stream = _streamSource?.GetInMemoryStream() ?? new MemoryStream();
            var emptyMemoryFolder = new MemoryFileEx(Path.Combine(Id, name), name, stream, this, _streamSource);
            emptyMemoryFolder.SetParent(this);

            var file = overwrite ? emptyMemoryFolder : (existingFile ?? emptyMemoryFolder);
            FolderContents[file.Id] = file;

            return file;
        }

        internal void SetParent(MemoryFolder memoryFolder)
        {
            Parent = memoryFolder;
        }
    }
}
