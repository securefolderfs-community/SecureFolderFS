using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Storage.Renamable;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.MemoryStorageEx
{
    /// <inheritdoc cref="MemoryFolder"/>
    public class MemoryFolderEx : MemoryFolder, IRenamableFolder
    {
        /// <inheritdoc/>
        public MemoryFolderEx(string id, string name)
            : base(id, name)
        {
            Parent = this;
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            var oldPath = storable.Id;
            var newPath = System.IO.Path.Combine(Id, newName);

            await Task.CompletedTask;
            if (storable is MemoryFileEx memoryFile)
            {
                // Use reflection to get the private _memoryStream member
                var memoryStreamField = typeof(MemoryFile).GetField("_memoryStream", BindingFlags.NonPublic | BindingFlags.Instance);
                var memoryStream = (MemoryStream?)memoryStreamField?.GetValue(memoryFile);
                if (memoryStream is null)
                    throw new MemberAccessException("Could not access _memoryStream");

                FolderContents.Remove(oldPath);
                var newFile = new MemoryFileEx(newPath, newName, memoryStream);
                newFile.SetParent(this);
                FolderContents.Add(newPath, newFile);

                return newFile;
            }
            else if (storable is IFolder)
            {
                FolderContents.Remove(oldPath);
                var newFolder = new MemoryFolderEx(newPath, newName);
                newFolder.SetParent(this);
                FolderContents.Add(newPath, newFolder);

                return newFolder;
            }

            throw new ArgumentOutOfRangeException(nameof(storable));
        }

        /// <inheritdoc/>
        public override async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingFolderKvp = FolderContents.FirstOrDefault(x => x.Value.Name == name && x.Value is IFolder);
            var existingFolder = existingFolderKvp.Value as IChildFolder;

            if (overwrite && existingFolder is not null)
                await DeleteAsync(existingFolder, cancellationToken);

            var emptyMemoryFolder = new MemoryFolderEx($"{Guid.NewGuid()}", name);
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

            var emptyMemoryFolder = new MemoryFileEx($"{Guid.NewGuid()}", name, new MemoryStream());
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
