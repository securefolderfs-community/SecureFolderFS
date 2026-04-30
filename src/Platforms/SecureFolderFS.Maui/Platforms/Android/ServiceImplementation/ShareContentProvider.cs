using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using OwlCore.Storage;
using SecureFolderFS.Shared.Helpers;
using Uri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <summary>
    /// A ContentProvider that serves virtualized files for sharing without creating temporary files.
    /// Files are streamed directly from their IFile implementation through a pipe.
    /// </summary>
    [ContentProvider(["${applicationId}.shareProvider"],
        Name = "securefolderfs.shareProvider",
        Enabled = true,
        Exported = false,
        GrantUriPermissions = true)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [Preserve(AllMembers = true)]
    public class ShareContentProvider : ContentProvider
    {
        /// <summary>
        /// How long a registered file is kept alive after the intent is launched,
        /// to accommodate apps that open multiple streams (e.g. type sniffing + actual read).
        /// </summary>
        private static readonly TimeSpan RegistrationTtl = TimeSpan.FromSeconds(20);
        
        private static readonly Dictionary<string, IFile> _registeredFiles = new();
        private static readonly Lock _lock = new();

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ShareContentProvider))]
        public ShareContentProvider()
        {
        }
        
        /// <summary>
        /// Registers a file for sharing and returns a content URI suitable for use in an Intent.
        /// The registration is automatically cleaned up after <see cref="RegistrationTtl"/>.
        /// </summary>
        /// <param name="context">The application context, used to resolve the authority.</param>
        /// <param name="file">The file to register.</param>
        /// <returns>A content URI pointing to the registered file.</returns>
        public static Uri? RegisterFileAndBuildUri(Context context, IFile file)
        {
            var fileId = Guid.NewGuid().ToString("N");
            lock (_lock)
                _registeredFiles[fileId] = file;
 
            // Schedule deferred cleanup - the call site must NOT call UnregisterFile manually,
            // because some apps open the stream more than once (type sniffing + actual read).
            _ = Task.Delay(RegistrationTtl)
                .ContinueWith(_ => UnregisterFile(fileId));
 
            var authority = $"{context.PackageName}.shareProvider";
            return Uri.Parse($"content://{authority}/{fileId}/{Uri.Encode(file.Name)}");
        }

        /// <summary>
        /// Unregisters a file after sharing is complete.
        /// </summary>
        /// <param name="fileId">The file identifier to unregister.</param>
        public static void UnregisterFile(string fileId)
        {
            lock (_lock)
                _registeredFiles.Remove(fileId);
        }

        /// <inheritdoc/>
        [Register("onCreate", "()Z", "GetOnCreateHandler")]
        public override bool OnCreate()
        {
            return true;
        }

        /// <inheritdoc/>
        public override ParcelFileDescriptor? OpenFile(Uri uri, string mode)
        {
            var fileId = uri.PathSegments?.FirstOrDefault();
            if (fileId is null)
                return null;

            IFile? file;
            lock (_lock)
            {
                if (!_registeredFiles.TryGetValue(fileId, out file))
                    return null;
            }

            // Create a reliable pipe and stream the file content through it
            var pipe = ParcelFileDescriptor.CreateReliablePipe();
            if (pipe is null || pipe.Length < 2)
                return null;

            var readSide = pipe[0];
            var writeSide = pipe[1];

            // Stream the content in a background thread
            _ = Task.Run(async () =>
            {
                try
                {
                    await using var fileStream = await file.OpenReadAsync();
                    using var outputStream = new ParcelFileDescriptor.AutoCloseOutputStream(writeSide);

                    int bytesRead;
                    var buffer = new byte[8192];
                    while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                }
                catch (Java.IO.IOException ex) when (IsBrokenPipe(ex))
                {
                    // Read side closed before we finished writing — normal for apps that
                    // sniff the stream type before opening it for real. Exit cleanly.
                    SafetyHelpers.NoFailure(writeSide.Close);
                }
                catch (Exception ex)
                {
                    // Signal the read side so the receiving app sees a real error
                    // instead of an unexpected EOF
                    SafetyHelpers.NoFailure(() => writeSide.CloseWithError(ex.Message));
                }
            });

            return readSide;
        }

        /// <inheritdoc/>
        public override ICursor? Query(Uri uri, string[]? projection, string? selection, string[]? selectionArgs, string? sortOrder)
        {
            var fileId = uri.PathSegments?.FirstOrDefault();
            if (fileId is null)
                return null;

            IFile? file;
            lock (_lock)
            {
                if (!_registeredFiles.TryGetValue(fileId, out file))
                    return null;
            }

            // Get the display name from the URI path (second segment)
            var fileName = uri.PathSegments?.ElementAtOrDefault(1) ?? file.Name;

            var columns = projection ?? [ IOpenableColumns.DisplayName, IOpenableColumns.Size ];
            var cursor = new MatrixCursor(columns);
            var row = cursor.NewRow();

            foreach (var column in columns)
            {
                switch (column)
                {
                    case IOpenableColumns.DisplayName:
                        row?.Add(fileName);
                        break;

                    case IOpenableColumns.Size:
                        row?.Add(null); // Size unknown for streams
                        break;

                    default:
                        row?.Add(null);
                        break;
                }
            }

            return cursor;
        }

        /// <inheritdoc/>
        public override string? GetType(Uri uri)
        {
            var fileId = uri.PathSegments?.FirstOrDefault();
            if (fileId is null)
                return "application/octet-stream";

            IFile? file;
            lock (_lock)
            {
                if (!_registeredFiles.TryGetValue(fileId, out file))
                    return "application/octet-stream";
            }

            return FileTypeHelper.GetMimeType(file.Name);
        }

        /// <inheritdoc/>
        public override Uri? Insert(Uri uri, ContentValues? values) => null;

        /// <inheritdoc/>
        public override int Delete(Uri uri, string? selection, string[]? selectionArgs) => 0;

        /// <inheritdoc/>
        public override int Update(Uri uri, ContentValues? values, string? selection, string[]? selectionArgs) => 0;
        
        private static bool IsBrokenPipe(Java.IO.IOException ex)
        {
            var msg = ex.Message;
            return msg is not null &&
                   (msg.Contains("EPIPE", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("Broken pipe", StringComparison.OrdinalIgnoreCase));
        }
    }
}

