using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Microsoft.Maui.Platform;
using OwlCore.Storage;
using static Android.Provider.DocumentsContract;
using static SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem.Projections;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    [ContentProvider(["${applicationId}.provider"],
        Name = "com.securefolderfs.securefolderfs.provider",
        Permission = "android.permission.MANAGE_DOCUMENTS",
        Enabled = true,
        Exported = true,
        GrantUriPermissions = true)]
    [IntentFilter(["android.content.action.DOCUMENTS_PROVIDER"])]
    internal sealed class FileSystemProvider : DocumentsProvider
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

                row.Add(Root.ColumnRootId, item.Rid);
                row.Add(Root.ColumnIcon, Constants.AndroidSaf.IC_LOCK_LOCK);
                row.Add(Root.ColumnTitle, item.StorageRoot.StorageName);
                row.Add(Root.ColumnDocumentId, GetDocumentIdForStorable(item.StorageRoot.Inner, item.Rid));
                row.Add(Root.ColumnFlags, (int)(DocumentRootFlags.LocalOnly | DocumentRootFlags.SupportsCreate));
            }

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

        private string GetDocumentIdForStorable(IStorable storable, string rid)
        {
            var safRoot = _rootCollection?.GetRootForRid(rid);
            if (storable.Id == safRoot?.StorageRoot.Inner.Id)
                return rid + ':';

            return $"{rid}:{storable.Id.Substring(0, safRoot?.StorageRoot.Inner.Id.Length ?? 0)}";
        }
    }
}
