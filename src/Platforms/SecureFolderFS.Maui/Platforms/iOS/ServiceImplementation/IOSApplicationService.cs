using Foundation;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class IOSApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } = "iOS - MAUI";
        
        /// <inheritdoc/>
        public override async Task OpenUriAsync(Uri uri)
        {
            var nsUrl = new NSUrl(uri.AbsoluteUri);
            _ = await UIApplication.SharedApplication.OpenUrlAsync(nsUrl, new());
        }
        
        /// <inheritdoc/>
        public override string GetSystemVersion()
        {
            return DeviceInfo.VersionString;
        }

        /// <inheritdoc/>
        public override Task TryRestartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
