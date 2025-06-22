using Android.App;
using Android.Content;
using Android.Provider;
using AndroidX.DocumentFile.Provider;
using Java.IO;
using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.Streams;
using SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties;
using SecureFolderFS.Storage.StorageProperties;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IChildFile"/>
    internal sealed class AndroidFile : AndroidStorable, IChildFile
    {
        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public override DocumentFile? Document { get; }

        public AndroidFile(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null, string? bookmarkId = null)
            : base(uri, activity, parent, permissionRoot, bookmarkId)
        {
            Document = DocumentFile.FromSingleUri(activity, uri);
            Name = Document?.Name ?? base.Name;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            if (IsVirtualFile(activity, Inner))
            {
                var stream = GetVirtualFileStream(activity, Inner, accessMode != FileAccess.Read);
                if (stream is null)
                    return Task.FromException<Stream>(new ArgumentException("No stream types available for '*/*' mime type."));

                return Task.FromResult(stream);
            }
            
            if (accessMode == FileAccess.Read)
            {
                var inStream = activity.ContentResolver?.OpenInputStream(Inner);
                if (inStream is null)
                    return Task.FromException<Stream>(new UnauthorizedAccessException("Could not open input stream."));
                
                return Task.FromResult(inStream);
            }
            else
            {
                var fd = activity.ContentResolver?.OpenFileDescriptor(Inner, "rwt");
                var fInChannel = new FileInputStream(fd.FileDescriptor).Channel;
                var fOutChannel = new FileOutputStream(fd.FileDescriptor).Channel;

                if (fInChannel is null || fOutChannel is null)
                    return Task.FromException<Stream>(
                        new ArgumentException("Could not open input and output streams."));

                var channelledStream = new ChannelledStream(fInChannel, fOutChannel);
                return Task.FromResult<Stream>(channelledStream);
            }
        }

        /// <inheritdoc/>
        public override Task<IBasicProperties> GetPropertiesAsync()
        {
            if (Document is null)
                return Task.FromException<IBasicProperties>(new ArgumentNullException(nameof(Document)));

            properties ??= new AndroidFileProperties(Document);
            return Task.FromResult(properties);
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
            if (!(mimeTypes?.Length >= 1))
                return null;

            var mimeType = mimeTypes[0];
            var asset = context.ContentResolver!.OpenTypedAssetFileDescriptor(uri, mimeType, null);
            var stream = isOutput
                ? asset?.CreateOutputStream()
                : asset?.CreateInputStream();

            return stream;
        }
    }
}
