using NWebDav.Server.Locking;
using NWebDav.Server.Storage;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.Paths.Native;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.EncryptingStorage2
{
    [Obsolete]
    internal sealed class EncryptingDiskStore : DiskStore
    {
        private readonly FileSystemSpecifics _specifics;

        public EncryptingDiskStore(string directory, FileSystemSpecifics specifics, bool isWritable = true, ILockingManager? lockingManager = null)
            : base(directory, isWritable, lockingManager)
        {
            _specifics = specifics;
        }

        public override Task<IDavStorable?> GetItemAsync(Uri uri, CancellationToken cancellationToken)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);

            // Check if it's a directory
            if (Directory.Exists(path))
                return Task.FromResult<IDavStorable?>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _specifics));

            // Check if it's a file
            if (File.Exists(path))
                return Task.FromResult<IDavStorable?>(new EncryptingDiskStoreFile(LockingManager, new FileInfo(path), IsWritable, _specifics));

            // The item doesn't exist
            return Task.FromResult<IDavStorable?>(null);
        }

        public override Task<IDavFolder?> GetCollectionAsync(Uri uri, CancellationToken cancellationToken)
        {
            // Determine the path from the uri
            var path = GetPathFromUri(uri);
            if (!Directory.Exists(path))
                return Task.FromResult<IDavFolder?>(null);

            // Return the item
            return Task.FromResult<IDavFolder?>(new EncryptingDiskStoreCollection(LockingManager, new DirectoryInfo(path), IsWritable, _specifics));
        }

        protected override string GetPathFromUri(Uri uri)
        {
            var path = base.GetPathFromUri(uri);
            return NativePathHelpers.GetCiphertextPath(path, _specifics);
        }
    }
}
