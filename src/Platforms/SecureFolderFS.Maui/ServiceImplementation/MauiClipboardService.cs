using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class MauiClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task SetTextAsync(string text, CancellationToken cancellationToken = default)
        {
            await Clipboard.SetTextAsync(text);
        }

        /// <inheritdoc/>
        public async Task<string?> GetTextAsync(CancellationToken cancellationToken)
        {
            return await Clipboard.GetTextAsync();
        }
    }
}
