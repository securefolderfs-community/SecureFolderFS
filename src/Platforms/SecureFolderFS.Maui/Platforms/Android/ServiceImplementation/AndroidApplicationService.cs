using Android.Content;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class AndroidApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override bool IsDesktop { get; } = false;

        /// <inheritdoc/>
        public override string Platform { get; } = "Android - MAUI";

        /// <inheritdoc/>
        public override Task OpenUriAsync(Uri uri)
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity?.PackageManager is null)
                return Task.CompletedTask;

            var androidUri = AndroidUri.Parse(uri.AbsoluteUri);
            var intent = new Intent(Intent.ActionView, androidUri);

            // Ensure that there's an activity to handle the intent
            if (intent.ResolveActivity(activity.PackageManager) != null)
                activity.StartActivity(intent);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override string GetSystemVersion()
        {
            return $"Android {DeviceInfo.VersionString}";
        }

        /// <inheritdoc/>
        public override Task TryRestartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
