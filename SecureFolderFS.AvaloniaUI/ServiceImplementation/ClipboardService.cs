using SecureFolderFS.Sdk.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            // TODO handle macOS
            if (OperatingSystem.IsLinux())
                return Task.FromResult(File.Exists("/usr/bin/xsel"));

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task SetTextAsync(string text, CancellationToken cancellationToken = default)
        {
            return TextCopy.ClipboardService.SetTextAsync(text, cancellationToken);
        }
    }
}