using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.MemoryStorageEx;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    internal sealed class MockFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(FilterOptions? filter, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();

            return new MemoryFileEx(guid, guid, new());
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(FilterOptions? filter, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var guid = Guid.NewGuid().ToString();

            return new MemoryFolderEx(guid, guid);
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
