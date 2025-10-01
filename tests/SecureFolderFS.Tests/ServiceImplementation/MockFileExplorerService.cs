using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.MemoryStorageEx;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    internal sealed class MockFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();

            return new MemoryFileEx(guid, guid, new(), null);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();

            return new MemoryFolderEx(guid, guid, null);
        }

        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            using var streamReader = new StreamReader(dataStream, leaveOpen: true);
            _ = await streamReader.ReadToEndAsync(cancellationToken);

            return true;
        }
    }
}
