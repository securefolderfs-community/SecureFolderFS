using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Java.IO;
using OwlCore.Storage;
using Application = Android.App.Application;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <summary>
    /// A ContentProvider that serves virtualized files for sharing without creating temporary files.
    /// Files are streamed directly from their IFile implementation through a pipe.
    /// </summary>
    [ContentProvider(["${applicationId}.shareProvider"],
        Name = "securefolderfs.shareProvider",
        Exported = false,
        GrantUriPermissions = true)]
    public sealed class ShareContentProvider : ContentProvider
    {
        private static readonly Dictionary<string, IFile> _registeredFiles = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Registers a file for sharing and returns a unique identifier.
        /// </summary>
        /// <param name="file">The file to register.</param>
        /// <returns>A unique identifier for the file.</returns>
        public static string RegisterFile(IFile file)
        {
            var fileId = Guid.NewGuid().ToString("N");
            lock (_lock)
            {
                _registeredFiles[fileId] = file;
            }

            return fileId;
        }

        /// <summary>
        /// Unregisters a file after sharing is complete.
        /// </summary>
        /// <param name="fileId">The file identifier to unregister.</param>
        public static void UnregisterFile(string fileId)
        {
            lock (_lock)
            {
                _registeredFiles.Remove(fileId);
            }
        }

        /// <inheritdoc/>
        public override bool OnCreate() => true;

        /// <inheritdoc/>
        public override ParcelFileDescriptor? OpenFile(global::Android.Net.Uri uri, string mode)
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

            // Create a pipe and stream the file content through it
            var pipe = ParcelFileDescriptor.CreatePipe();
            if (pipe is null || pipe.Length < 2)
                return null;

            var readSide = pipe[0];
            var writeSide = pipe[1];

            // Stream the content in a background thread
            _ = Task.Run(async () =>
            {
                try
                {
                    await using var outputStream = new ParcelFileDescriptor.AutoCloseOutputStream(writeSide);
                    await using var fileStream = await file.OpenReadAsync();
                    await fileStream.CopyToAsync(outputStream);
                }
                catch
                {
                    // Silently handle errors during streaming
                }
                finally
                {
                    // Clean up the registration after streaming
                    UnregisterFile(fileId);
                }
            });

            return readSide;
        }

        /// <inheritdoc/>
        public override ICursor? Query(global::Android.Net.Uri uri, string[]? projection, string? selection, string[]? selectionArgs, string? sortOrder)
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

            // Get the filename from the URI path (second segment)
            var fileName = uri.PathSegments?.ElementAtOrDefault(1) ?? file.Name;

            var columns = projection ?? new[] { OpenableColumns.DisplayName, OpenableColumns.Size };
            var cursor = new MatrixCursor(columns);
            var row = cursor.NewRow();

            foreach (var column in columns)
            {
                switch (column)
                {
                    case OpenableColumns.DisplayName:
                        row?.Add(fileName);
                        break;
                    case OpenableColumns.Size:
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
        public override string? GetType(global::Android.Net.Uri uri)
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

            return Shared.Helpers.FileTypeHelper.GetMimeType(file) ?? "application/octet-stream";
        }

        /// <inheritdoc/>
        public override global::Android.Net.Uri? Insert(global::Android.Net.Uri uri, ContentValues? values) => null;

        /// <inheritdoc/>
        public override int Delete(global::Android.Net.Uri uri, string? selection, string[]? selectionArgs) => 0;

        /// <inheritdoc/>
        public override int Update(global::Android.Net.Uri uri, ContentValues? values, string? selection, string[]? selectionArgs) => 0;
    }
}

