using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Storage.Renamable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.SystemStorageEx
{
    /// <inheritdoc cref="SystemFolder"/>
    public class SystemFolderEx : SystemFolder, IRenamableFolder
    {
        /// <inheritdoc/>
        public SystemFolderEx(string path)
            : base(path)
        {
        }

        /// <inheritdoc/>
        public SystemFolderEx(DirectoryInfo info)
            : base(info)
        {
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            var oldPath = storable.Id;
            var newPath = System.IO.Path.Combine(Id, newName);

            await Task.CompletedTask;
            if (storable is IFile)
            {
                File.Move(oldPath, newPath);
                return new SystemFileEx(newPath);
            }
            else if (storable is IFolder)
            {
                Directory.Move(oldPath, newPath);
                return new SystemFolderEx(newPath);
            }

            throw new ArgumentOutOfRangeException(nameof(storable));
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in base.GetItemsAsync(type, cancellationToken))
            {
                if (SpecialNames.IllegalNames.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                yield return item switch
                {
                    IFile file => new SystemFileEx(file.Id),
                    IFolder folder => new SystemFolderEx(folder.Id),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <inheritdoc/>
        public override async Task<IStorableChild> GetItemRecursiveAsync(string id, CancellationToken cancellationToken = default)
        {
            return await base.GetItemRecursiveAsync(id, cancellationToken) switch
            {
                IFile file => new SystemFileEx(file.Id),
                IFolder folder => new SystemFolderEx(folder.Id),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc/>
        public override async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            return await base.GetItemAsync(id, cancellationToken) switch
            {
                IFile file => new SystemFileEx(file.Id),
                IFolder folder => new SystemFolderEx(folder.Id),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc/>
        public override async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await base.GetFirstByNameAsync(name, cancellationToken) switch
            {
                IFile file => new SystemFileEx(file.Id),
                IFolder folder => new SystemFolderEx(folder.Id),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc/>
        public override async Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, CancellationToken cancellationToken, CreateCopyOfDelegate fallback)
        {
            var file = await base.CreateCopyOfAsync(fileToCopy, overwrite, cancellationToken, fallback);
            return new SystemFileEx(file.Id);
        }

        /// <inheritdoc/>
        public override async Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken, MoveFromDelegate fallback)
        {
            var file = await base.MoveFromAsync(fileToMove, source, overwrite, cancellationToken, fallback);
            return new SystemFileEx(file.Id);
        }

        /// <inheritdoc/>
        public override async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var folder = await base.CreateFolderAsync(name, overwrite, cancellationToken);
            return new SystemFolderEx(folder.Id);
        }

        /// <inheritdoc/>
        public override async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var file = await base.CreateFileAsync(name, overwrite, cancellationToken);
            return new SystemFileEx(file.Id);
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parent = Directory.GetParent(Path);
            return Task.FromResult<IFolder?>(parent != null ? new SystemFolderEx(parent) : null);
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetRootAsync(CancellationToken cancellationToken = default)
        {
            var root = new DirectoryInfo(Path).Root;
            return Task.FromResult<IFolder?>(new SystemFolderEx(root));
        }
    }
}
