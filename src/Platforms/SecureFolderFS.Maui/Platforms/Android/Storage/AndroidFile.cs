using Android.App;
using Android.Content;
using Android.Provider;
using Android.Runtime;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.Streams;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IChildFile"/>
    internal sealed class AndroidFile : AndroidStorable, IChildFile
    {
        /// <inheritdoc/>
        protected override DocumentFile? Document { get; }

        public AndroidFile(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null, string? bookmarkId = null)
            : base(uri, activity, parent, permissionRoot, bookmarkId)
        {
            Document = DocumentFile.FromSingleUri(activity, uri);
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            Stream? stream;
            if (IsVirtualFile(activity, Inner))
            {
                stream = GetVirtualFileStream(activity, Inner, accessMode != FileAccess.Read);
                if (stream is null)
                    return Task.FromException<Stream>(new ArgumentException("No stream types available for '*/*' mime type."));

                return Task.FromResult(stream);
            }

            if (accessMode == FileAccess.Read)
            {
                stream = activity.ContentResolver?.OpenInputStream(Inner);
            }
            else
            {
                var inputStream = (activity.ContentResolver?.OpenInputStream(Inner) as InputStreamInvoker)?.BaseInputStream;
                var outputStream = (activity.ContentResolver?.OpenOutputStream(Inner) as OutputStreamInvoker)?.BaseOutputStream;
                if (inputStream is null || outputStream is null)
                    return Task.FromException<Stream>(new UnauthorizedAccessException($"Could not open a stream to: '{Id}'."));

                var combinedInputStream = new InputOutputStream(inputStream, outputStream, GetFileSize(activity.ContentResolver, Inner));
                stream = combinedInputStream;
            }
            
            if (stream is null)
                return Task.FromException<Stream>(new UnauthorizedAccessException($"Could not open a stream to: '{Id}'."));

            return Task.FromResult(stream);
        }

        private static bool IsVirtualFile(Context context, AndroidUri uri)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(24))
                return false;

            if (!DocumentsContract.IsDocumentUri(context, uri))
                return false;

            var value = GetColumnValue(context, uri, DocumentsContract.Document.ColumnFlags);
            if (string.IsNullOrEmpty(value) || !int.TryParse(value, out var flagsInt))
                return false;

            var flags = (DocumentContractFlags)flagsInt;
            return flags.HasFlag(DocumentContractFlags.VirtualDocument);
        }

        private static Stream? GetVirtualFileStream(Context context, AndroidUri uri, bool isOutput)
        {
            var mimeTypes = context.ContentResolver?.GetStreamTypes(uri, "*/*");
            if (mimeTypes?.Length >= 1)
            {
                var mimeType = mimeTypes[0];
                var asset = context.ContentResolver!
                    .OpenTypedAssetFileDescriptor(uri, mimeType, null);

                var stream = isOutput
                    ? asset?.CreateOutputStream()
                    : asset?.CreateInputStream();

                return stream;
            }

            return null;
        }
        
        private static long GetFileSize(ContentResolver? contentResolver, AndroidUri uri)
        {
            if (contentResolver is null)
                return 0L;
            
            try
            {
                // Try to get file size using content resolver
                using var cursor = contentResolver.Query(uri, null, null, null, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int sizeIndex = cursor.GetColumnIndex(IOpenableColumns.Size);
                    if (sizeIndex != -1)
                    {
                        return cursor.GetLong(sizeIndex);
                    }
                }
            }
            catch
            {
                // Fallback method if content resolver fails
            }

            // If size can't be determined, return -1
            return -1;
        }
    }
}
