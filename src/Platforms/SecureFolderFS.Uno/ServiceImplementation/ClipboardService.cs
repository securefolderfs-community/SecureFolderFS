using SecureFolderFS.Sdk.Services;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);

            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();

            return Task.CompletedTask;
        }
    }
}
