using System.Web;
using Android.App;
using Android.Content;
using Android.Provider;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Storage;
using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem;
using SecureFolderFS.Maui.Platforms.Android.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.VirtualFileSystem;
using AndroidUri = Android.Net.Uri;
using AOSEnvironment = Android.OS.Environment;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class AndroidFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public async Task<IFile?> PickFileAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
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
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
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
        public Task<bool> TryOpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            try
            {
                var context = MainActivity.Instance;
                if (context is null)
                    return Task.FromResult(false);

                // Find the SafRoot that owns this folder by matching VFSRoot
                IVFSRoot? vfsRoot = null;
                if (folder is IWrapper<IFolder> wrapper)
                {
                    // Walk wrappers to find the CryptoStorable which holds FileSystemSpecifics -> IVFSRoot
                    // Alternatively, match via FileSystemManager directly
                    vfsRoot = FileSystemManager.Instance.FileSystems
                        .FirstOrDefault(x => (x.VirtualizedRoot as IWrapper<IFolder>)?.GetDeepestWrapper().Inner.Id == wrapper.Inner.Id || IsOwnedByRoot(wrapper.Inner, x));
                }

                if (vfsRoot is null)
                    return Task.FromResult(false);

                // Now find the corresponding SafRoot for this IVFSRoot
                var safRoot = RootCollection.Instance?.Roots.FirstOrDefault(r => r.StorageRoot == vfsRoot);
                if (safRoot is null)
                    return Task.FromResult(false);

                var intent = new Intent(Intent.ActionView);
                var documentUri = AndroidUri.Parse($"content://{Core.MobileFS.Constants.Android.FileSystem.AUTHORITY}/root/{safRoot.RootId}");
                intent.SetData(documentUri);
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);
                context.StartActivity(intent);

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        private static bool IsOwnedByRoot(IFolder folder, IVFSRoot root)
        {
            var virtualizedRootInner = (root.VirtualizedRoot as IWrapper<IFolder>)?.GetDeepestWrapper().Inner;
            return folder.Id == virtualizedRootInner?.Id
                   || folder.Id.StartsWith(virtualizedRootInner?.Id ?? string.Empty, StringComparison.Ordinal);
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
