using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IShareService"/>
    /// <remarks>
    /// This is a fallback implementation. Platform-specific implementations 
    /// (AndroidShareService, IOSShareService) are used for actual sharing.
    /// </remarks>
    internal class MauiShareService : IShareService
    {
        /// <inheritdoc/>
        public virtual async Task ShareTextAsync(string text, string title)
        {
            await Share.Default.RequestAsync(new ShareTextRequest()
            {
                Text = text,
                Title = title
            });
        }

        /// <inheritdoc/>
        public virtual Task ShareFileAsync(IFile file)
        {
            // This fallback implementation cannot share virtualized files
            // Platform-specific implementations should be used instead
            throw new PlatformNotSupportedException("File sharing requires platform-specific implementation for virtualized files.");
        }
    }
}
