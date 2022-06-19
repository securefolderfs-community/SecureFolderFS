using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IFile"/>
    internal sealed class WindowsStorageFile : WindowsBaseStorage<StorageFile>, IFile
    {
        /// <inheritdoc/>
        public string Extension { get; }

        public WindowsStorageFile(StorageFile storage)
            : base(storage)
        {
            Extension = storage.FileType;
        }

        /// <inheritdoc/>
        public Task<Stream?> OpenStreamAsync(FileAccess access)
        {
            return OpenStreamAsync(access, FileShare.None);
        }

        /// <inheritdoc/>
        public async Task<Stream?> OpenStreamAsync(FileAccess access, FileShare share)
        {
            try
            {
                var fileAccessMode = GetFileAccessMode(access);
                var storageOpenOptions = GetStorageOpenOptions(share);

                var winrtStream = await storage.OpenAsync(fileAccessMode, storageOpenOptions);

                return winrtStream.AsStream();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Stream> GetThumbnailStreamAsync(uint requestedSize)
        {
            var winrtStream = await storage.GetThumbnailAsync(ThumbnailMode.SingleItem, requestedSize);
            return winrtStream.AsStreamForRead();
        }

        /// <inheritdoc/>
        public override async Task<IFolder?> GetParentAsync()
        {
            try
            {
                var parent = await storage.GetParentAsync();
                return new WindowsStorageFolder(parent);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static FileAccessMode GetFileAccessMode(FileAccess access)
        {
            return access switch
            {
                FileAccess.Read => FileAccessMode.Read,
                FileAccess.Write => FileAccessMode.ReadWrite,
                FileAccess.ReadWrite => FileAccessMode.ReadWrite,
                _ => throw new ArgumentOutOfRangeException(nameof(access))
            };
        }

        private static StorageOpenOptions GetStorageOpenOptions(FileShare share)
        {
            return share switch
            {
                FileShare.Read => StorageOpenOptions.AllowOnlyReaders,
                FileShare.Write => StorageOpenOptions.AllowReadersAndWriters,
                FileShare.ReadWrite => StorageOpenOptions.AllowReadersAndWriters,
                FileShare.Inheritable => StorageOpenOptions.None,
                FileShare.Delete => StorageOpenOptions.None,
                FileShare.None => StorageOpenOptions.None,
                _ => throw new ArgumentOutOfRangeException(nameof(share))
            };
        }
    }
}
