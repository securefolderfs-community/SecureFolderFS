using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.MemoryStorageEx;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    internal sealed class MockFileExplorerService : IFileExplorerService
    {
        private byte[] _savedFileBytes = [];

        /// <inheritdoc/>
        public async Task<IEnumerable<IStorable>> PickGalleryItemsAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();

            return [ new MemoryFileEx(guid, guid, new(), null) ];
        }

        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();
            var stream = new MemoryStream(_savedFileBytes, writable: false);

            return new MemoryFileEx(guid, guid, stream, null);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();

            return new MemoryFolderEx(guid, guid, null);
        }

        /// <inheritdoc/>
        public Task<bool> TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            _ = suggestedName;
            _ = filter;

            if (dataStream.CanSeek)
                dataStream.Position = 0L;

            await using var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream, cancellationToken);
            _savedFileBytes = memoryStream.ToArray();

            return true;
        }
    }
}
