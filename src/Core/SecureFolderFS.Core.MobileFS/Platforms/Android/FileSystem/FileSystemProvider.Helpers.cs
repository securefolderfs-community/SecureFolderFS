using Android.Database;
using Android.Provider;
using Android.Webkit;
using OwlCore.Storage;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
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

            var rootFolderId = BuildDocumentId(safRoot, safRoot.StorageRoot.PlaintextRoot);
            row.Add(Root.ColumnRootId, safRoot.RootId);
            row.Add(Root.ColumnDocumentId, rootFolderId);
            row.Add(Root.ColumnTitle, safRoot.StorageRoot.Options.VolumeName);
            row.Add(Root.ColumnIcon, iconRid);

            // SupportsIsChild is required for ACTION_OPEN_DOCUMENT_TREE to work against this root
            row.Add(Root.ColumnFlags, (int)(DocumentRootFlags.LocalOnly | DocumentRootFlags.SupportsCreate | DocumentRootFlags.SupportsIsChild));

            return true;
        }

        private async Task<bool> AddDocumentAsync(MatrixCursor matrix, IStorable storable, SafRoot safRoot, string documentId)
        {
            var row = matrix.NewRow();
            if (row is null)
                return false;

            AddDocumentId();
            AddDisplayName();
            await AddSizeAsync();
            await AddLastModifiedAsync();
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
            async Task AddLastModifiedAsync()
            {
                if (storable is not ILastModifiedAt lastModifiedAt)
                    return;

                var dateModified = await SafetyHelpers.NoFailureAsync(() => lastModifiedAt.LastModifiedAt.GetValueAsync());
                if (dateModified is not null)
                    row.Add(Document.ColumnLastModified, new DateTimeOffset(dateModified.Value.ToUniversalTime()).ToUnixTimeMilliseconds());
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

                    var typeHint = FileTypeHelper.GetTypeHint(storable);
                    if (typeHint is TypeHint.Image or TypeHint.Media)
                        baseFlags |= DocumentContractFlags.SupportsThumbnail;
                }
                else
                {
                    baseFlags |= DocumentContractFlags.DirPrefersGrid;
                    if (!safRoot.StorageRoot.Options.IsReadOnly)
                        baseFlags |= DocumentContractFlags.DirSupportsCreate;
                }

                row.Add(Document.ColumnFlags, (int)baseFlags);
            }
            void AddMimeType() => row.Add(Document.ColumnMimeType, GetMimeForStorable(storable));
            void AddDocumentId() => row.Add(Document.ColumnDocumentId, documentId);
            void AddDisplayName() => row.Add(Document.ColumnDisplayName, string.IsNullOrEmpty(storable.Name)
                ? safRoot.StorageRoot.Options.VolumeName
                : storable.Name);
        }

        /// <summary>
        /// Builds the document ID (in 'rootId:plaintextPath' format) of <paramref name="storable"/> under <paramref name="safRoot"/>.
        /// </summary>
        private static string BuildDocumentId(SafRoot safRoot, IStorable storable)
        {
            return storable.Id == safRoot.StorageRoot.PlaintextRoot.Id
                ? $"{safRoot.RootId}:"
                : $"{safRoot.RootId}:{storable.Id}";
        }

        /// <summary>
        /// Gets the document ID of the parent of the item identified by <paramref name="documentId"/>.
        /// </summary>
        private static string? GetParentDocumentId(string documentId)
        {
            var split = documentId.Split(':', 2);
            if (split.Length < 2)
                return null;

            var path = split[1].TrimEnd('/');
            var separatorIndex = path.LastIndexOf('/');

            return separatorIndex <= 0 ? $"{split[0]}:" : $"{split[0]}:{path[..separatorIndex]}";
        }

        /// <summary>
        /// Resolves the <see cref="SafRoot"/> that the document identified by <paramref name="documentId"/> belongs to.
        /// </summary>
        /// <remarks>
        /// The root is always derived from the document ID. It must never be inferred from a storable's
        /// path, because plaintext paths are not unique across roots (every root starts at '/').
        /// </remarks>
        private SafRoot? GetSafRootForDocumentId(string documentId)
        {
            var split = documentId.Split(':', 2);
            if (split.Length < 2)
                return null;

            return _rootCollection?.GetSafRootForRootId(split[0]);
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

            // Return the base folder if the path is empty
            if (string.IsNullOrEmpty(path))
                return safRoot.StorageRoot.PlaintextRoot;

            return SafetyHelpers.NoFailureResult(() => safRoot.StorageRoot.PlaintextRoot.GetItemByRelativePathAsync(path).ConfigureAwait(false).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Notifies content observers that the children of <paramref name="parentDocumentId"/> have changed.
        /// </summary>
        private void NotifyChildDocumentsChange(string? parentDocumentId)
        {
            if (parentDocumentId is null)
                return;

            var childrenUri = BuildChildDocumentsUri(Constants.Android.FileSystem.AUTHORITY, parentDocumentId);
            if (childrenUri is not null)
                Context?.ContentResolver?.NotifyChange(childrenUri, null);
        }

        private static string GetMimeForStorable(IStorable storable)
        {
            if (storable is IFolder)
                return Document.MimeTypeDir;

            // Get the extension
            var extension = IOPath.GetExtension(storable.Name);
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            // Remove the starting dot
            return MimeTypeMap.Singleton?.GetMimeTypeFromExtension(extension.Substring(1).ToLowerInvariant()) ?? "application/octet-stream";
        }
    }
}
