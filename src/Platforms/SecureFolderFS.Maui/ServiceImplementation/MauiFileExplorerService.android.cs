using System.Web;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Storage;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;

#if ANDROID
using AndroidUri = Android.Net.Uri;
using Android.App;
using Android.Content;
using Android.Provider;
using AndroidX.Activity;
#endif

namespace SecureFolderFS.Maui.ServiceImplementation
{
    internal sealed partial class MauiFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public partial async Task<bool> SaveFileAsync(string suggestedName, Stream dataStream, IDictionary<string, string>? filter, CancellationToken cancellationToken = default)
        {
            var fileSaver = FileSaver.Default;
            var result = await fileSaver.SaveAsync(suggestedName, dataStream, cancellationToken);

            return result.IsSuccessful;
        }

        /// <inheritdoc/>
        public partial async Task<IFile?> PickFileAsync(IEnumerable<string>? filter, CancellationToken cancellationToken = default)
        {
            var filePicker = FilePicker.Default;
            var result = await filePicker.PickAsync();
            if (result is null)
                return null;

            AddAndroidBookmark(result.FullPath);

            return new SystemFile(new FileInfo(result.FullPath));
        }

        /// <inheritdoc/>
        public partial async Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = FolderPicker.Default;
            var result = await folderPicker.PickAsync(cancellationToken);
            if (!result.IsSuccessful)
                return null;

            AddAndroidBookmark(result.Folder.Path);

            return new SystemFolder(new DirectoryInfo(result.Folder.Path));
#if ANDROID

            Folder? folder = null;

            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                var statusRead = await Permissions.RequestAsync<Permissions.StorageRead>().WaitAsync(cancellationToken).ConfigureAwait(false);
                if (statusRead is not PermissionStatus.Granted)
                    throw new PermissionException("Storage permission is not granted.");
            }

            var initialPath = AndroidPathExtensions.GetExternalDirectory();
            if (Android.OS.Environment.ExternalStorageDirectory is not null)
                initialPath = initialPath.Replace(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, string.Empty, StringComparison.InvariantCulture);


            var initialFolderUri = AndroidUri.Parse("content://com.android.externalstorage.documents/document/primary%3A" + HttpUtility.UrlEncode(initialPath));
            var intent = new Intent(Intent.ActionOpenDocumentTree);
            intent.PutExtra(DocumentsContract.ExtraInitialUri, initialFolderUri);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            intent.AddFlags(ActivityFlags.GrantWriteUriPermission);

            //IntermediateActivity

            // RequestCodeFolderPicker 0x000007D0
            await MainActivity.Instance.StartAsync(intent, 0x000007D0, onResult: OnResult);
            //MainActivity.Instance.StartActivityForResult(intent, 0x000007D0);

            return new SystemFolder(folder?.Path) ?? throw new FolderPickerException("Unable to get folder.");

            void OnResult(Intent resultIntent)
            {
                var path = resultIntent.Data.ToPhysicalPath() ?? throw new FolderPickerException($"Unable to resolve absolute path or retrieve contents of URI.");
                folder = new Folder(path, Path.GetFileName(path));
            }
#endif

        }

        private static void AddAndroidBookmark(string id)
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity is not null && AndroidUri.Parse(id) is { } uri)
            {
                activity.ContentResolver?.TakePersistableUriPermission(uri,
                    ActivityFlags.GrantWriteUriPermission |
                    ActivityFlags.GrantReadUriPermission);
            }
#endif
        }
    }
}
