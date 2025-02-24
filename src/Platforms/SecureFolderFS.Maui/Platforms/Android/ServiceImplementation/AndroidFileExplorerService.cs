using System.Web;
using Android.App;
using Android.Content;
using Android.Provider;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.Android.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Storage.Pickers;
using AndroidUri = Android.Net.Uri;
using AOSEnvironment = Android.OS.Environment;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class AndroidFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(FilterOptions? filter, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            var intent = new Intent(Intent.ActionOpenDocument)
                .AddCategory(Intent.CategoryOpenable)
                .PutExtra(Intent.ExtraAllowMultiple, false)
                .SetType("*/*");

            var pickerIntent = Intent.CreateChooser(intent, "Select file");
            
            // TODO: Determine if GrantReadUriPermission and GrantWriteUriPermission are needed for access persistence

            // FilePicker 0x2AF9
            var result = await StartActivityAsync(pickerIntent, 0x2AF9);
            if (result is null || MainActivity.Instance is null)
                return null;

            return new AndroidFile(result, MainActivity.Instance);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(FilterOptions? filter, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            var initialPath = AndroidPathExtensions.GetExternalDirectory();
            if (AOSEnvironment.ExternalStorageDirectory is not null)
                initialPath = initialPath.Replace(AOSEnvironment.ExternalStorageDirectory.AbsolutePath, string.Empty, StringComparison.InvariantCulture);

            var initialFolderUri = AndroidUri.Parse("content://com.android.externalstorage.documents/document/primary%3A" + HttpUtility.UrlEncode(initialPath));
            var intent = new Intent(Intent.ActionOpenDocumentTree);

            intent.PutExtra(DocumentsContract.ExtraInitialUri, initialFolderUri);
            if (offerPersistence)
            {
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                intent.AddFlags(ActivityFlags.GrantWriteUriPermission);
            }

            // RequestCodeFolderPicker 0x000007D0
            var result = await StartActivityAsync(intent, 0x000007D0);
            if (result is null || MainActivity.Instance is null)
                return null;

            return new AndroidFolder(result, MainActivity.Instance);
        }

        /// <inheritdoc/>
        public Task TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            // TODO: Try to implement opening in android file explorer
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var fileSaver = FileSaver.Default;
            var result = await fileSaver.SaveAsync(suggestedName, dataStream, cancellationToken);

            return result.IsSuccessful;
        }

        private async Task<AndroidUri?> StartActivityAsync(Intent? pickerIntent, int requestCode)
        {
            AndroidUri? resultUri = null;
            var tcs = new TaskCompletionSource<Intent?>();

            var activity = MainActivity.Instance;
            if (activity is null)
                return null;

            activity.ActivityResult += OnActivityResult;
            activity.StartActivityForResult(pickerIntent, requestCode);

            var result = await tcs.Task;
            if (result?.Data is { } uri)
                resultUri = uri;

            if (result?.HasExtra("error") == true)
                throw new Exception(result.GetStringExtra("error"));

            return resultUri;

            void OnActivityResult(int rC, Result resultCode, Intent? data)
            {
                if (requestCode != rC)
                    return;

                activity.ActivityResult -= OnActivityResult;
                _ = tcs.TrySetResult(resultCode == Result.Ok ? data : null);
            }
        }
    }
}
