using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using static Android.Provider.DocumentsContract;
using static SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem.Projections;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    [ContentProvider(["${applicationId}.provider"],
        Name = "com.securefolderfs.securefolderfs.fileProvider",
        Permission = "android.permission.MANAGE_DOCUMENTS",
        Enabled = true,
        Exported = true,
        GrantUriPermissions = true)]
    [IntentFilter(["android.content.action.DOCUMENTS_PROVIDER"])]
    internal sealed class FileSystemProvider : DocumentsProvider
    {
        /// <inheritdoc/>
        public override bool OnCreate()
        {
            return true;
        }

        /// <inheritdoc/>
        public override ICursor? QueryRoots(string[]? projection)
        {
            var matrix = new MatrixCursor(projection ?? DefaultRootProjection);

            // TODO(fs): Check if there are any available (unlocked) vaults.
            // Return an empty cursor if there are no vaults unlocked

            var row = matrix.NewRow();
            if (row is null)
                return null;

            // Temporary root properties (including IDs) to be replaced with multiple-roots approach
            row.Add(Root.ColumnRootId, Constants.FILE_SYSTEM_ROOT_ID);
            row.Add(Root.ColumnIcon, 0x0108002f); // ic_lock_lock 0x0108002f
            row.Add(Root.ColumnTitle, "Test MobileFS (SecureFolderFS)");
            row.Add(Root.ColumnDocumentId,  Constants.ROOT_DIRECTORY_ID);
            row.Add(Root.ColumnFlags, (int)(DocumentRootFlags.LocalOnly | DocumentRootFlags.SupportsCreate));

            return matrix;
        }

        /// <inheritdoc/>
        public override ParcelFileDescriptor? OpenDocument(string? documentId, string? mode, CancellationSignal? signal)
        {
            return null;
        }

        /// <inheritdoc/>
        public override ICursor? QueryChildDocuments(string? parentDocumentId, string[]? projection, string? sortOrder)
        {
            return QueryDocument(parentDocumentId, projection);
        }

        /// <inheritdoc/>
        public override ICursor? QueryDocument(string? documentId, string[]? projection)
        {
            return new MatrixCursor(projection ?? DefaultDocumentProjection);
        }
    }
}
