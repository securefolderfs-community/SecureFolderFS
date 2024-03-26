using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    internal sealed partial class MauiFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            // TODO: Try to implement opening in mobile file explorer
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public partial Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public partial Task<IFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public partial Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default);
    }
}
