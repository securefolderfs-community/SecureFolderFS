using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
using static Android.Provider.DocumentsContract;
using static SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem.Projections;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    [ContentProvider(["${applicationId}.provider"],
        Name = "org.securefolderfs.securefolderfs.provider",
        Permission = "android.permission.MANAGE_DOCUMENTS",
        Enabled = true,
        Exported = true,
        GrantUriPermissions = true)]
    [IntentFilter(["android.content.action.DOCUMENTS_PROVIDER"])]
    internal sealed partial class FileSystemProvider : DocumentsProvider
    {
        private RootCollection? _rootCollection;

        /// <inheritdoc/>
        public override bool OnCreate()
        {
            _rootCollection = new(Platform.AppContext);
            return true;
        }

        /// <inheritdoc/>
        public override ICursor? QueryRoots(string[]? projection)
        {
            var matrix = new MatrixCursor(projection ?? DefaultRootProjection);
            foreach (var item in _rootCollection?.Roots ?? Enumerable.Empty<SafRoot>())
            {
                var row = matrix.NewRow();
                if (row is null)
                    return null;

                row.Add(Root.ColumnRootId, item.RootId);
                row.Add(Root.ColumnIcon, Constants.Android.Saf.IC_LOCK_LOCK);
                row.Add(Root.ColumnTitle, item.StorageRoot.Options.VolumeName);
                row.Add(Root.ColumnDocumentId, GetDocumentIdForStorable(item.StorageRoot.Inner, item.RootId));
                row.Add(Root.ColumnFlags, (int)(DocumentRootFlags.LocalOnly | DocumentRootFlags.SupportsCreate));
            }

            return matrix;
        }

        /// <inheritdoc/>
        public override ParcelFileDescriptor? OpenDocument(string? documentId, string? mode, CancellationSignal? signal)
        {
            if (documentId is null)
                return null;

            var file = GetStorableForDocumentId(documentId);
            if (file is not IChildFile childFile)
                return null;

            var fileAccess = ToFileAccess(mode);
            if (fileAccess is null)
                return null;

            var stream = childFile.TryOpenStreamAsync(fileAccess.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            if (stream is null)
                return null;

            var pipe = ParcelFileDescriptor.CreatePipe();
            if (pipe is null)
                return null;

            var readingPipe = pipe[0];
            var writingPipe = pipe[1];

            try
            {
                using var output = new ParcelFileDescriptor.AutoCloseOutputStream(writingPipe);
                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
                output.Flush();
            }
            catch (IOException e)
            {
                System.Diagnostics.Debug.WriteLine("Error writing to pipe: " + e.Message);
            }

            return readingPipe;

            static FileAccess? ToFileAccess(string? mode)
            {
                if (string.IsNullOrEmpty(mode))
                    return null;

                return mode switch
                {
                    "r" => FileAccess.Read,
                    "w" => FileAccess.Write,
                    "rw" => FileAccess.ReadWrite,
                    _ => FileAccess.Read
                };
            }
        }

        /// <inheritdoc/>
        public override ICursor? QueryDocument(string? documentId, string[]? projection)
        {
            var matrix = new MatrixCursor(projection ?? DefaultDocumentProjection);
            if (documentId is null)
                return matrix;

            var storable = GetStorableForDocumentId(documentId);
            if (storable is null)
                return matrix;

            AddStorable(matrix, storable, documentId);
            return matrix;
        }

        /// <inheritdoc/>
        public override ICursor? QueryChildDocuments(string? parentDocumentId, string[]? projection, string? sortOrder)
        {
            var matrix = new MatrixCursor(projection ?? DefaultDocumentProjection);
            if (parentDocumentId is null)
                return matrix;

            var parent = GetStorableForDocumentId(parentDocumentId);
            if (parent is not IFolder folder)
                return matrix;

            var items = folder.GetItemsAsync().ToArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var item in items)
            {
                AddStorable(matrix, item, null);
            }

            return matrix;
        }
    }
}
