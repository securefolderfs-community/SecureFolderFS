using Android.Database;
using Android.Provider;
using Android.Webkit;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.StorageProperties;
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

            var rootFolderId = GetDocumentIdForStorable(safRoot.StorageRoot.VirtualizedRoot, safRoot.RootId);
            row.Add(DocumentsContract.Root.ColumnRootId, safRoot.RootId);
            row.Add(DocumentsContract.Root.ColumnDocumentId, rootFolderId);
            row.Add(DocumentsContract.Root.ColumnTitle, safRoot.StorageRoot.Options.VolumeName);
            row.Add(DocumentsContract.Root.ColumnIcon, iconRid);
            row.Add(DocumentsContract.Root.ColumnFlags, (int)(DocumentRootFlags.LocalOnly | DocumentRootFlags.SupportsCreate));

            return true;
        }

        private async Task<bool> AddDocumentAsync(MatrixCursor matrix, IStorable storable, string? documentId)
        {
            documentId ??= GetDocumentIdForStorable(storable, null);
            var row = matrix.NewRow();
            if (row is null)
                return false;

            var safRoot = _rootCollection?.GetSafRootForStorable(storable);
            if (safRoot is null)
                return false;

            AddDocumentId();
            AddDisplayName();
            await AddSizeAsync();
            AddMimeType();
            AddFlags();

            return true;

            async Task AddSizeAsync()
            {
                if (storable is not IFile file)
                {
                    row.Add(Document.ColumnSize, 0);
                    return;
                }

                var size = await file.GetSizeAsync();
                row.Add(Document.ColumnSize, size);
            }
            void AddFlags()
            {
                var baseFlags = (DocumentContractFlags)0;
                if (!safRoot.StorageRoot.Options.IsReadOnly)
                {
                    baseFlags |= DocumentContractFlags.SupportsCopy
                                 | DocumentContractFlags.SupportsMove
                                 | DocumentContractFlags.SupportsRename
                                 | DocumentContractFlags.SupportsDelete
                                 | DocumentContractFlags.SupportsRemove;
                }

                if (storable is IFile)
                {
                    if (!safRoot.StorageRoot.Options.IsReadOnly)
                        baseFlags |= DocumentContractFlags.SupportsWrite;

                    row.Add(Document.ColumnFlags, (int)baseFlags);
                }
                else
                {
                    baseFlags |= DocumentContractFlags.DirPrefersGrid;
                    if (!safRoot.StorageRoot.Options.IsReadOnly)
                        baseFlags |= DocumentContractFlags.DirSupportsCreate;

                    row.Add(Document.ColumnFlags, (int)baseFlags);
                }
            }
            void AddMimeType() => row.Add(Document.ColumnMimeType, GetMimeForStorable(storable));
            void AddDocumentId() => row.Add(Document.ColumnDocumentId, documentId);
            void AddDisplayName()
            {
                if (string.IsNullOrEmpty(storable.Name))
                {
                    var safRoot = _rootCollection?.GetSafRootForRootId(documentId?.Split(':')[0] ?? string.Empty);
                    row.Add(Document.ColumnDisplayName, safRoot?.StorageRoot.Options.VolumeName ?? storable.Name);
                }
                else
                    row.Add(Document.ColumnDisplayName, storable.Name);
            }
        }

        private string? GetDocumentIdForStorable(IStorable storable, string? rootId)
        {
            var safRoot = rootId is not null
                ? _rootCollection?.GetSafRootForRootId(rootId)
                : _rootCollection?.GetSafRootForStorable(storable);

            if (safRoot is null)
                return null;

            if (storable.Id == safRoot.StorageRoot.VirtualizedRoot.Id)
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
                return safRoot.StorageRoot.VirtualizedRoot;

            return safRoot.StorageRoot.VirtualizedRoot.GetItemByRelativePathAsync(path).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private string GetMimeForStorable(IStorable storable)
        {
            if (storable is IFolder)
                return Document.MimeTypeDir;

            // Get the extension
            var extension = IOPath.GetExtension(storable.Name);
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            // Remove the starting . (dot)
            return MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension.Substring(1)) ?? string.Empty;
        }
    }
}
