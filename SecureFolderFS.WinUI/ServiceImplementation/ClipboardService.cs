using SecureFolderFS.Sdk.Services;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public Task<bool> IsClipboardAvailableAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task SetTextAsync(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);

            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();

            return Task.CompletedTask;
        }
    }
}
