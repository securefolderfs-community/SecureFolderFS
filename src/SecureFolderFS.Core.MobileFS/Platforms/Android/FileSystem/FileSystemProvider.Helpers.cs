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
        private bool AddStorable(MatrixCursor matrix, IStorable storable, string? documentId)
        {
            documentId ??= GetDocumentIdForStorable(storable, null);
            var row = matrix.NewRow();
            if (row is null)
                return false;

            // TODO(saf): Implement columns
            row.Add(Document.ColumnSize, 6);
            row.Add(Document.ColumnLastModified, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            row.Add(Document.ColumnDocumentId, documentId);
            row.Add(Document.ColumnMimeType, GetMimeForStorable(storable));

            if (storable is not IFolder)
                row.Add(Document.ColumnFlags, (int)(DocumentContractFlags.SupportsDelete | DocumentContractFlags.SupportsWrite));
            else
                row.Add(Document.ColumnFlags, (int)(DocumentContractFlags.DirSupportsCreate | DocumentContractFlags.DirPrefersLastModified | DocumentContractFlags.DirPrefersGrid));

            if (string.IsNullOrEmpty(storable.Name))
            {
                var safRoot = _rootCollection?.GetRootForRootId(documentId?.Split(':')[0] ?? string.Empty);
                row.Add(Document.ColumnDisplayName, safRoot?.StorageRoot.StorageName ?? storable.Name);
            }
            else
                row.Add(Document.ColumnDisplayName, storable.Name);

            return true;
        }

        private string? GetDocumentIdForStorable(IStorable storable, string? rootId)
        {
            var safRoot = rootId is not null
                ? _rootCollection?.GetRootForRootId(rootId)
                : _rootCollection?.GetRootForStorable(storable);

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

            var split = documentId.Split(':');
            if (split.Length < 2)
                throw new FileNotFoundException("Invalid document ID: " + documentId);

            var rootId = split[0];
            var safRoot = _rootCollection.GetRootForRootId(rootId);
            if (safRoot is null)
                return null;

            var path = split[1];
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
