using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.OS.Storage;
using Android.Provider;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
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
                AddRoot(matrix, item, Constants.Android.Saf.IC_LOCK_LOCK);
            }

            return matrix;
        }

        /// <inheritdoc/>
        public override string? CreateDocument(string? parentDocumentId, string? mimeType, string? displayName)
        {
            parentDocumentId = parentDocumentId == "null" ? null : parentDocumentId;
            if (parentDocumentId is null || displayName is null)
                return null;

            var parentStorable = GetStorableForDocumentId(parentDocumentId);
            if (parentStorable is not IModifiableFolder parentFolder)
                return null;

            var createdItem = (IStorableChild)(mimeType switch
            {
                DocumentsContract.Document.MimeTypeDir => parentFolder.CreateFolderAsync(displayName, false)
                    .ConfigureAwait(false).GetAwaiter().GetResult(),
                _ => parentFolder.CreateFileAsync(displayName, false).ConfigureAwait(false).GetAwaiter().GetResult()
            });

            var rootId = parentDocumentId.Split(':', 2)[0];
            return $"{rootId}:{createdItem.Id}";
        }

        /// <inheritdoc/>
        public override ParcelFileDescriptor? OpenDocument(string? documentId, string? mode, CancellationSignal? signal)
        {
            documentId = documentId == "null" ? null : documentId;
            if (documentId is null)
                return null;

            var file = GetStorableForDocumentId(documentId);
            if (file is not IChildFile childFile)
                return null;

            var fileAccess = ToFileAccess(mode);
            var stream = childFile.TryOpenStreamAsync(fileAccess).ConfigureAwait(false).GetAwaiter().GetResult();
            if (stream is null)
                return null;

            var storageManager = (StorageManager?)this.Context?.GetSystemService(Context.StorageService);
            if (storageManager is null)
                return null;

            var parcelFileMode = ToParcelFileMode(mode);
            return storageManager.OpenProxyFileDescriptor(parcelFileMode, new ReadWriteCallbacks(stream), new Handler(Looper.MainLooper));

            static ParcelFileMode ToParcelFileMode(string? fileMode)
            {
                return fileMode switch
                {
                    "r" => ParcelFileMode.ReadOnly,
                    "w" => ParcelFileMode.WriteOnly,
                    "rw" => ParcelFileMode.ReadWrite,
                    _ => throw new ArgumentException($"Unsupported mode: {fileMode}")
                };
            }

            static FileAccess ToFileAccess(string? fileMode)
            {
                return fileMode switch
                {
                    "r" => FileAccess.Read,
                    "w" => FileAccess.Write,
                    "rw" => FileAccess.ReadWrite,
                    _ => throw new ArgumentException($"Unsupported mode: {fileMode}")
                };
            }
        }

        /// <inheritdoc/>
        public override ICursor? QueryDocument(string? documentId, string[]? projection)
        {
            documentId = documentId == "null" ? null : documentId;
            var matrix = new MatrixCursor(projection ?? DefaultDocumentProjection);
            if (documentId is null)
                return matrix;

            var storable = GetStorableForDocumentId(documentId);
            if (storable is null)
                return matrix;

            AddDocument(matrix, storable, documentId);
            return matrix;
        }

        /// <inheritdoc/>
        public override ICursor? QueryChildDocuments(string? parentDocumentId, string[]? projection, string? sortOrder)
        {
            parentDocumentId = parentDocumentId == "null" ? null : parentDocumentId;
            var matrix = new MatrixCursor(projection ?? DefaultDocumentProjection);
            if (parentDocumentId is null)
                return matrix;

            var parent = GetStorableForDocumentId(parentDocumentId);
            if (parent is not IFolder folder)
                return matrix;

            var items = folder.GetItemsAsync().ToArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var item in items)
            {
                AddDocument(matrix, item, null);
            }

            return matrix;
        }
    }
}
