#if APP_PLATFORM_PRESENT
using SecureFolderFS.Sdk.AppPlatform.Helpers;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="BrowserAuthProvider"/>
    internal sealed class MauiOidcProvider : BrowserAuthProvider
    {
        /// <inheritdoc/>
        protected override async Task OpenSystemBrowserAsync(string authUrl, CancellationToken ct)
        {
            await Browser.Default.OpenAsync(authUrl, BrowserLaunchMode.SystemPreferred);
        }
    }
}
#endif
