using System.Runtime.CompilerServices;
using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage
{
    /// <inheritdoc cref="IChildFolder"/>
    internal sealed class IOSFolder : IOSStorable, IRenamableFolder, IChildFolder, IGetFirstByName
    {
        public IOSFolder(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null, string? bookmarkId = null)
            : base(url, parent, permissionRoot, bookmarkId)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            try
            {
                permissionRoot.StartAccessingSecurityScopedResource();

                var tcs = new TaskCompletionSource<IReadOnlyList<IStorableChild>>();
                new NSFileCoordinator().CoordinateRead(Inner,
                    NSFileCoordinatorReadingOptions.WithoutChanges,
                    out var error,
                    uri =>
                    {
                        var content = NSFileManager.DefaultManager.GetDirectoryContent(uri, null, NSDirectoryEnumerationOptions.None, out var error2);
                        if (error2 is null)
                        {
                            var items = content.Select(x => NewStorage(x, this, permissionRoot)).ToArray();
                            tcs.TrySetResult(items);
                        }
                        else
                            tcs.TrySetException(new NSErrorException(error2));
                    });

                if (error is not null)
                    throw new NSErrorException(error);

                var items = await tcs.Task;
                foreach (var item in items)
                    yield return item;
            }
            finally
            {
                permissionRoot.StopAccessingSecurityScopedResource();
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                permissionRoot.StartAccessingSecurityScopedResource();

                var isDirectory = false;
                var itemPath = Path.Combine(Id, name);

                if (!NSFileManager.DefaultManager.FileExists(itemPath, ref isDirectory))
                    throw new FileNotFoundException($"Item '{name}' not found in folder '{Id}'.");

                return NewStorage(new NSUrl(itemPath, isDirectory), this, permissionRoot);
            }
            finally
            {
                permissionRoot.StopAccessingSecurityScopedResource();
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (item is not IWrapper<NSUrl> iosWrapper)
                throw new ArgumentException("Storable item must wrap an NSUrl.", nameof(item));

            try
            {
                permissionRoot.StartAccessingSecurityScopedResource();
                if (!NSFileManager.DefaultManager.Remove(iosWrapper.Inner, out var error))
                    throw new NSErrorException(error);
            }
            finally
            {
                permissionRoot.StopAccessingSecurityScopedResource();
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            try
            {
                permissionRoot.StartAccessingSecurityScopedResource();

                var path = Path.Combine(Id, name);
                NSFileAttributes? attributes = null;

                if (NSFileManager.DefaultManager.CreateDirectory(path, false, attributes, out var error))
                    return new IOSFolder(new NSUrl(path, true), this, permissionRoot);

                if (error is not null)
                    throw new NSErrorException(error);

                throw new UnauthorizedAccessException("Attempt to create folder failed.");
            }
            finally
            {
                permissionRoot.StopAccessingSecurityScopedResource();
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!permissionRoot.StartAccessingSecurityScopedResource())
                    throw new UnauthorizedAccessException("Could not create iOS file.");

                var path = Path.Combine(Id, name);
                NSFileAttributes? attributes = null;

                if (NSFileManager.DefaultManager.CreateFile(path, new NSData(), attributes))
                    return new IOSFile(new NSUrl(path, false), this, permissionRoot);

                throw new UnauthorizedAccessException("Attempt to create file failed.");
            }
            finally
            {
                permissionRoot.StopAccessingSecurityScopedResource();
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!permissionRoot.StartAccessingSecurityScopedResource())
                    throw new UnauthorizedAccessException("Could not rename iOS item.");

                if (storable is not IWrapper<NSUrl> iosWrapper)
                    throw new ArgumentException("Storable item must wrap an NSUrl.", nameof(storable));

                var originalUrl = iosWrapper.Inner;
                var originalPath = originalUrl.Path;
                var newPath = Path.Combine(Path.GetDirectoryName(originalPath)!, newName);
                var newUrl = new NSUrl(newPath, storable is IFolder);

                // Ensure the destination doesn't already exist
                if (NSFileManager.DefaultManager.FileExists(newPath))
                    throw new IOException($"A file or folder with the name '{newName}' already exists.");

                if (!NSFileManager.DefaultManager.Move(originalUrl, newUrl, out var error))
                    throw new NSErrorException(error);

                return NewStorage(newUrl, this, permissionRoot);
            }
            finally
            {
                permissionRoot.StopAccessingSecurityScopedResource();
                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public override Task<IBasicProperties> GetPropertiesAsync()
        {
            properties ??= new IOSFolderProperties(Inner, permissionRoot);
            return Task.FromResult(properties);
        }
    }
}
