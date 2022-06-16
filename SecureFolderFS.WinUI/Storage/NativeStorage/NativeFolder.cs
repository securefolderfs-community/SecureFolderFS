using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.WinUI.Storage.NativeStorage
{
    /// <inheritdoc cref="IFolder"/>
    internal sealed class NativeFolder : NativeBaseStorage, IFolder
    {
        public NativeFolder(string path)
            : base(path)
        {
        }

        /// <inheritdoc/>
        public Task<IFile?> CreateFileAsync(string desiredName)
        {
            return CreateFileAsync(desiredName, CreationCollisionOption.FailIfExists);
        }

        /// <inheritdoc/>
        public async Task<IFile?> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            _ = options;

            try
            {
                var path = System.IO.Path.Combine(Path, desiredName);
                await File.Create(path).DisposeAsync();

                return new NativeFile(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<IFolder?> CreateFolderAsync(string desiredName)
        {
            return CreateFolderAsync(desiredName, CreationCollisionOption.FailIfExists);
        }

        /// <inheritdoc/>
        public Task<IFolder?> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            _ = options;

            try
            {
                var path = System.IO.Path.Combine(Path, desiredName);
                _ = Directory.CreateDirectory(path);

                return Task.FromResult<IFolder?>(new NativeFolder(path));
            }
            catch (Exception)
            {
                return Task.FromResult<IFolder?>(null);
            }
        }

        /// <inheritdoc/>
        public Task<IFile?> GetFileAsync(string fileName)
        {
            var path = System.IO.Path.Combine(Path, fileName);

            if (!File.Exists(path))
                return Task.FromResult<IFile?>(null);

            return Task.FromResult<IFile?>(new NativeFile(path));
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetFolderAsync(string folderName)
        {
            var path = System.IO.Path.Combine(Path, folderName);

            if (!File.Exists(path))
                return Task.FromResult<IFolder?>(null);

            return Task.FromResult<IFolder?>(new NativeFolder(path));
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFile> GetFilesAsync()
        {
            foreach (var item in Directory.EnumerateFiles(Path))
            {
                yield return new NativeFile(item);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFolder> GetFoldersAsync()
        {
            foreach (var item in Directory.EnumerateDirectories(Path))
            {
                yield return new NativeFolder(item);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IBaseStorage> GetStorageAsync()
        {
            foreach (var item in Directory.EnumerateFileSystemEntries(Path))
            {
                if (File.Exists(item))
                    yield return new NativeFile(item);

                if (Directory.Exists(item))
                    yield return new NativeFolder(item);

                yield break;
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override Task<bool> RenameAsync(string newName, NameCollisionOption options)
        {
            if (string.IsNullOrEmpty(newName))
                return Task.FromResult(false);

            var parentPath = System.IO.Path.GetDirectoryName(Path);
            if (string.IsNullOrEmpty(parentPath))
                return Task.FromResult(false);

            try
            {
                var newPath = System.IO.Path.Combine(parentPath, newName);
                Directory.Move(Path, newPath);

                Path = newPath;
                Name = newName;

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc/>
        public override Task<bool> DeleteAsync(bool permanently)
        {
            _ = permanently;

            try
            {
                Directory.Delete(Path, true);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
