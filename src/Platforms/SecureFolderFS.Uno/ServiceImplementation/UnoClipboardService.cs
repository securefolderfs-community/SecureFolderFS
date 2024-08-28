using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using Windows.ApplicationModel.DataTransfer;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class UnoClipboardService : IClipboardService
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

        /// <inheritdoc/>
        public async Task<string?> GetTextAsync(CancellationToken cancellationToken)
        {
            var dataPackage = Clipboard.GetContent();
            if (dataPackage is null)
                return null;

            if (!dataPackage.Contains(StandardDataFormats.Text))
                throw new FormatException("The data is not in correct format.");

            return await dataPackage.GetTextAsync().AsTask(cancellationToken).ConfigureAwait(false);
        }
    }
}
