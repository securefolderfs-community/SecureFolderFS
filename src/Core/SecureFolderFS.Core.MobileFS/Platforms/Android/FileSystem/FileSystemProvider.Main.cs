using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Util;
using Microsoft.Maui.Platform;
using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.Helpers;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using static SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem.Projections;
using Point = Android.Graphics.Point;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    [ContentProvider(["${applicationId}.provider"],
        Name = Constants.Android.FileSystem.AUTHORITY,
        Permission = "android.permission.MANAGE_DOCUMENTS",
        Enabled = true,
        Exported = true,
        GrantUriPermissions = true)]
    [IntentFilter(["android.content.action.DOCUMENTS_PROVIDER"])]
    internal sealed partial class FileSystemProvider : DocumentsProvider
    {
        private readonly StorageManagerCompat _storageManager;
        private RootCollection? _rootCollection;

        public FileSystemProvider()
        {
            _storageManager = new(Platform.AppContext);
        }

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
            var rid = MauiApplication.Current.GetDrawableId("app_icon.png");
            if (rid == 0)
                rid = Constants.Android.Saf.IC_LOCK_LOCK;

            foreach (var item in _rootCollection?.Roots ?? Enumerable.Empty<SafRoot>())
                AddRoot(matrix, item, rid);

            return matrix;
        }

        /// <inheritdoc/>
        public override bool IsChildDocument(string? parentDocumentId, string? documentId)
        {
            // Required for ACTION_OPEN_DOCUMENT_TREE - the system validates every access
            // through a tree grant by checking the ancestry of the target document
            parentDocumentId = parentDocumentId == "null" ? null : parentDocumentId;
            documentId = documentId == "null" ? null : documentId;
            if (parentDocumentId is null || documentId is null)
                return false;

            var parentSplit = parentDocumentId.Split(':', 2);
            var childSplit = documentId.Split(':', 2);
            if (parentSplit.Length < 2 || childSplit.Length < 2)
                return false;

            // Both documents must belong to the same root
            if (parentSplit[0] != childSplit[0])
                return false;

            var parentPath = parentSplit[1].TrimEnd('/');
            var childPath = childSplit[1];

            // The root folder is an ancestor of every document within it
            if (parentPath.Length == 0)
                return true;

            return childPath.StartsWith(parentPath, StringComparison.Ordinal)
                   && (childPath.Length == parentPath.Length || childPath[parentPath.Length] == '/');
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

            var safRoot = GetSafRootForDocumentId(parentDocumentId);
            if (safRoot is null)
                return null;

            if (safRoot.StorageRoot.Options.IsReadOnly)
                return null;

            // De-duplicate the display name, as creating over an existing document
            // would return the existing item and its contents would get overwritten
            var finalName = displayName;
            var counter = 1;
            while (counter < 32 && parentFolder.TryGetFirstByNameAsync(finalName).ConfigureAwait(false).GetAwaiter().GetResult() is not null)
                finalName = $"{Path.GetFileNameWithoutExtension(displayName)} ({counter++}){Path.GetExtension(displayName)}";

            var createdItem = (IStorableChild)(mimeType switch
            {
                DocumentsContract.Document.MimeTypeDir => parentFolder.CreateFolderAsync(finalName).ConfigureAwait(false).GetAwaiter().GetResult(),
                _ => parentFolder.CreateFileAsync(finalName).ConfigureAwait(false).GetAwaiter().GetResult()
            });

            NotifyChildDocumentsChange(parentDocumentId);

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
                throw new Java.IO.FileNotFoundException($"No document found for '{documentId}'.");

            var safRoot = GetSafRootForDocumentId(documentId);
            if (safRoot is null)
                throw new Java.IO.FileNotFoundException($"No root found for '{documentId}'.");

            var fileAccess = ToFileAccess(mode);
            if (safRoot.StorageRoot.Options.IsReadOnly && fileAccess.HasFlag(FileAccess.Write))
                return null;

            var stream = childFile.TryOpenStreamAsync(fileAccess).ConfigureAwait(false).GetAwaiter().GetResult();
            if (stream is null)
                return null;

            // The 'w' family of modes replaces the existing content
            if (IsTruncateMode(mode))
                stream.SetLength(0L);

            var handlerThread = new HandlerThread("ProxyFD-" + documentId);
            handlerThread.Start();
            return _storageManager.OpenProxyFileDescriptor(
                ToParcelFileMode(mode),
                new ReadWriteCallbacks(stream, handlerThread),
                new Handler(handlerThread.Looper!));

            static bool IsTruncateMode(string? fileMode)
            {
                return fileMode is "w" or "wt" or "rwt";
            }

            static ParcelFileMode ToParcelFileMode(string? fileMode)
            {
                return fileMode switch
                {
                    "r" => ParcelFileMode.ReadOnly,
                    "w" or "wt" => ParcelFileMode.WriteOnly,
                    "wa" => ParcelFileMode.WriteOnly | ParcelFileMode.Append,
                    "rw" or "rwt" => ParcelFileMode.ReadWrite,
                    _ => throw new ArgumentException($"Unsupported mode: {fileMode}.")
                };
            }

            static FileAccess ToFileAccess(string? fileMode)
            {
                return fileMode switch
                {
                    "r" => FileAccess.Read,
                    // Writes require read access as well (encrypted chunks are read-modify-write)
                    "w" or "wt" or "wa" => FileAccess.ReadWrite,
                    "rw" or "rwt" => FileAccess.ReadWrite,
                    _ => throw new ArgumentException($"Unsupported mode: {fileMode}.")
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
            var safRoot = GetSafRootForDocumentId(documentId);
            if (storable is null || safRoot is null)
                throw new Java.IO.FileNotFoundException($"No document found for '{documentId}'.");

            AddDocumentAsync(matrix, storable, safRoot, documentId).ConfigureAwait(false).GetAwaiter().GetResult();
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
            var safRoot = GetSafRootForDocumentId(parentDocumentId);
            if (parent is not IFolder folder || safRoot is null)
                throw new Java.IO.FileNotFoundException($"No document found for '{parentDocumentId}'.");

            var items = folder.GetItemsAsync().ToArrayAsyncImpl().ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var item in items)
            {
                // Build the child ID from the parent's root. Inferring the root from
                // the item is ambiguous when multiple vaults are unlocked
                var childDocumentId = BuildDocumentId(safRoot, item);
                AddDocumentAsync(matrix, item, safRoot, childDocumentId).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            // Register for change notifications so file managers refresh automatically
            var childrenUri = DocumentsContract.BuildChildDocumentsUri(Constants.Android.FileSystem.AUTHORITY, parentDocumentId);
            if (childrenUri is not null && Context?.ContentResolver is { } contentResolver)
                matrix.SetNotificationUri(contentResolver, childrenUri);

            return matrix;
        }

        /// <inheritdoc/>
        public override void RemoveDocument(string? documentId, string? parentDocumentId)
        {
            DeleteDocument(documentId);
        }

        /// <inheritdoc/>
        public override void DeleteDocument(string? documentId)
        {
            documentId = documentId == "null" ? null : documentId;
            if (documentId is null)
                return;

            var storable = GetStorableForDocumentId(documentId);
            if (storable is not IStorableChild storableChild)
                throw new Java.IO.FileNotFoundException($"No document found for '{documentId}'.");

            var safRoot = GetSafRootForDocumentId(documentId);
            if (safRoot is null)
                return;

            if (safRoot.StorageRoot.Options.IsReadOnly)
                return;

            var parentFolder = storableChild.GetParentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (parentFolder is not IModifiableFolder modifiableFolder)
                return;

            // Revoke permissions first
            RevokeDocumentPermission(documentId);

            // Perform deletion
            modifiableFolder.DeleteAsync(storableChild).ConfigureAwait(false).GetAwaiter().GetResult();
            NotifyChildDocumentsChange(GetParentDocumentId(documentId));
        }

        /// <inheritdoc/>
        public override string? MoveDocument(string? sourceDocumentId, string? sourceParentDocumentId, string? targetParentDocumentId)
        {
            sourceDocumentId = sourceDocumentId == "null" ? null : sourceDocumentId;
            sourceParentDocumentId = sourceParentDocumentId == "null" ? null : sourceParentDocumentId;
            targetParentDocumentId = targetParentDocumentId == "null" ? null : targetParentDocumentId;
            if (sourceDocumentId is null || targetParentDocumentId is null || sourceParentDocumentId is null)
                return null;

            var destinationStorable = GetStorableForDocumentId(targetParentDocumentId);
            if (destinationStorable is not IModifiableFolder destinationFolder)
                return null;

            var safRoot = GetSafRootForDocumentId(targetParentDocumentId);
            if (safRoot is null)
                return null;

            if (safRoot.StorageRoot.Options.IsReadOnly)
                return null;

            var sourceParentStorable = GetStorableForDocumentId(sourceParentDocumentId);
            if (sourceParentStorable is not IModifiableFolder sourceParentFolder)
                return null;

            var sourceStorable = GetStorableForDocumentId(sourceDocumentId);
            switch (sourceStorable)
            {
                case IChildFile file:
                {
                    var movedFile = destinationFolder.MoveFileImmediatelyFrom(file, sourceParentFolder, false, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    NotifyChildDocumentsChange(sourceParentDocumentId);
                    NotifyChildDocumentsChange(targetParentDocumentId);

                    return BuildDocumentId(safRoot, movedFile);
                }

                case IModifiableFolder folder:
                {
                    var movedFolder = destinationFolder.MoveFromAsync(folder, sourceParentFolder, false, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    NotifyChildDocumentsChange(sourceParentDocumentId);
                    NotifyChildDocumentsChange(targetParentDocumentId);

                    return BuildDocumentId(safRoot, movedFolder);
                }

                default: return null;
            }
        }

        /// <inheritdoc/>
        public override string? RenameDocument(string? documentId, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return null;

            documentId = documentId == "null" ? null : documentId;
            if (documentId is null)
                return null;

            var storable = GetStorableForDocumentId(documentId);
            if (storable is not IStorableChild storableChild)
                throw new Java.IO.FileNotFoundException($"No document found for '{documentId}'.");

            var safRoot = GetSafRootForDocumentId(documentId);
            if (safRoot is null)
                return null;

            if (safRoot.StorageRoot.Options.IsReadOnly)
                return null;

            var parentFolder = storableChild.GetParentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (parentFolder is not IRenamableFolder renamableFolder)
                return null;

            var renamedItem = renamableFolder.RenameAsync(storableChild, displayName).ConfigureAwait(false).GetAwaiter().GetResult();
            NotifyChildDocumentsChange(GetParentDocumentId(documentId));

            // The contract expects the new document ID (never an underlying URI)
            return BuildDocumentId(safRoot, renamedItem);
        }

        /// <inheritdoc/>
        public override string? CopyDocument(string? sourceDocumentId, string? targetParentDocumentId)
        {
            sourceDocumentId = sourceDocumentId == "null" ? null : sourceDocumentId;
            targetParentDocumentId = targetParentDocumentId == "null" ? null : targetParentDocumentId;
            if (sourceDocumentId is null || targetParentDocumentId is null)
                return null;

            var destinationStorable = GetStorableForDocumentId(targetParentDocumentId);
            if (destinationStorable is not IModifiableFolder destinationFolder)
                return null;

            var safRoot = GetSafRootForDocumentId(targetParentDocumentId);
            if (safRoot is null)
                return null;

            if (safRoot.StorageRoot.Options.IsReadOnly)
                return null;

            var sourceStorable = GetStorableForDocumentId(sourceDocumentId);
            switch (sourceStorable)
            {
                case IFile file:
                {
                    var copiedFile = destinationFolder.CreateCopyOfAsync(file, false).ConfigureAwait(false).GetAwaiter().GetResult();
                    NotifyChildDocumentsChange(targetParentDocumentId);

                    return BuildDocumentId(safRoot, copiedFile);
                }

                case IFolder folder:
                {
                    var copiedFolder = destinationFolder.CreateCopyOfAsync(folder, false).ConfigureAwait(false).GetAwaiter().GetResult();
                    NotifyChildDocumentsChange(targetParentDocumentId);

                    return BuildDocumentId(safRoot, copiedFolder);
                }

                default: return null;
            }
        }

        /// <inheritdoc/>
        public override AssetFileDescriptor? OpenDocumentThumbnail(string? documentId, Point? sizeHint, CancellationSignal? signal)
        {
            documentId = documentId == "null" ? null : documentId;
            if (documentId is null)
                return null;

            var storable = GetStorableForDocumentId(documentId);
            if (storable is not IFile file)
                return null;

            var typeHint = FileTypeHelper.GetTypeHint(file);
            if (typeHint is not (TypeHint.Image or TypeHint.Media))
                return null;

            try
            {
                if (signal?.IsCanceled ?? false)
                    return null;

                // Honor sizeHint from the caller instead of always using a fixed size
                var size = sizeHint is not null
                    ? (uint)Math.Max(sizeHint.X, sizeHint.Y)
                    : 300U;

                // Capture at one second rather than the very first frame, which is often
                // black (fade-ins). ClosestSync clamps the position for shorter videos
                using var inputStream = file.OpenReadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var thumbnailStream = typeHint is TypeHint.Media
                    ? ThumbnailHelpers.GenerateVideoThumbnailAsync(inputStream, TimeSpan.FromSeconds(1), (int)size, (int)size).ConfigureAwait(false).GetAwaiter().GetResult()
                    : ThumbnailHelpers.GenerateImageThumbnailAsync(inputStream, size).ConfigureAwait(false).GetAwaiter().GetResult();

                if (signal?.IsCanceled ?? false)
                {
                    thumbnailStream.Dispose();
                    return null;
                }

                var twoWayPipe = ParcelFileDescriptor.CreatePipe();
                if (twoWayPipe is null)
                    return null;

                var output = new ParcelFileDescriptor.AutoCloseOutputStream(twoWayPipe[1]);
                Task.Run(() =>
                {
                    try
                    {
                        var buffer = new byte[8192];
                        using (thumbnailStream)
                        {
                            int read;
                            while ((read = thumbnailStream.Read(buffer, 0, buffer.Length)) > 0)
                                output.Write(buffer, 0, read);
                        }

                        output.Flush();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(nameof(FileSystemProvider), $"Failed to write thumbnail to pipe. {ex}");
                    }
                    finally
                    {
                        // Always close the write end so the read end gets a clean EOF,
                        // even if the drain throws midway
                        output.Close();
                    }
                });

                return new AssetFileDescriptor(twoWayPipe[0], 0, AssetFileDescriptor.UnknownLength);
            }
            catch (Exception ex)
            {
                Log.Error(nameof(FileSystemProvider), $"Failed to read file. {ex}");
                return null;
            }
        }
    }
}
