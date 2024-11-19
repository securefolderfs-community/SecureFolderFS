using Android.Database;
using Android.Provider;
using Android.Webkit;
using OwlCore.Storage;
using static Android.Provider.DocumentsContract;
using IOPath = System.IO.Path;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    internal sealed partial class FileSystemProvider
    {
        private bool AddRoot(MatrixCursor matrix, SafRoot safRoot, int iconRid)
        {
            var row = matrix.NewRow();
            if (row is null)
                return false;

            var rootFolderId = GetDocumentIdForStorable(safRoot.StorageRoot.Inner, safRoot.RootId);
            row.Add(DocumentsContract.Root.ColumnRootId, safRoot.RootId);
            row.Add(DocumentsContract.Root.ColumnDocumentId, rootFolderId);
            row.Add(DocumentsContract.Root.ColumnTitle, safRoot.StorageRoot.Options.VolumeName);
            row.Add(DocumentsContract.Root.ColumnIcon, iconRid);
            row.Add(DocumentsContract.Root.ColumnFlags, (int)(DocumentRootFlags.LocalOnly | DocumentRootFlags.SupportsCreate));

            return true;
        }
        
        private bool AddDocument(MatrixCursor matrix, IStorable storable, string? documentId)
        {
            documentId ??= GetDocumentIdForStorable(storable, null);
            var row = matrix.NewRow();
            if (row is null)
                return false;

            // TODO(saf): Implement columns
            row.Add(Document.ColumnDocumentId, documentId);
            if (string.IsNullOrEmpty(storable.Name))
            {
                var safRoot = _rootCollection?.GetSafRootForRootId(documentId?.Split(':')[0] ?? string.Empty);
                row.Add(Document.ColumnDisplayName, safRoot?.StorageRoot.Options.VolumeName ?? storable.Name);
            }
            else
                row.Add(Document.ColumnDisplayName, storable.Name);
            
            row.Add(Document.ColumnSize, 6);
            row.Add(Document.ColumnMimeType, GetMimeForStorable(storable));

            if (storable is IFile)
                row.Add(Document.ColumnFlags, (int)(DocumentContractFlags.SupportsDelete | DocumentContractFlags.SupportsWrite));
            else
                row.Add(Document.ColumnFlags, (int)(DocumentContractFlags.DirSupportsCreate | DocumentContractFlags.DirPrefersGrid));
            
            return true;
        }

        private string? GetDocumentIdForStorable(IStorable storable, string? rootId)
        {
            var safRoot = rootId is not null
                ? _rootCollection?.GetSafRootForRootId(rootId)
                : _rootCollection?.GetSafRootForStorable(storable);

            if (safRoot is null)
                return null;

            if (storable.Id == safRoot.StorageRoot.Inner.Id)
                return $"{safRoot.RootId}:";

            return $"{safRoot.RootId}:{storable.Id}";
        }

        private IStorable? GetStorableForDocumentId(string documentId)
        {
            if (_rootCollection is null)
                return null;

            // Split the documentId into two:
            // 1. RootID - The source root of the document provider where the item belongs
            // 2. Path - The path to an item
            var split = documentId.Split(':', 2);
            if (split.Length < 2)
                return null;

            // Extract RootID and Path
            var rootId = split[0];
            var path = split[1];
            
            // Get root
            var safRoot = _rootCollection.GetSafRootForRootId(rootId);
            if (safRoot is null)
                return null;

            // Return base folder if the path is empty
            if (string.IsNullOrEmpty(path))
                return safRoot.StorageRoot.Inner;

            return safRoot.StorageRoot.Inner.GetItemByRelativePathAsync(path).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private string GetMimeForStorable(IStorable storable)
        {
            if (storable is IFolder)
                return Document.MimeTypeDir;

            // Get extension without the starting . (dot)
            var extension = IOPath.GetExtension(storable.Name).Substring(1);
            return MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension) ?? string.Empty;
        }
    }
}
