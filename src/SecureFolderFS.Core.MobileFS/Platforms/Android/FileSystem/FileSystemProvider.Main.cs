using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Java.IO;
using OwlCore.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Storage.Renamable;
using static SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem.Projections;
using Point = Android.Graphics.Point;

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

            var safRoot = _rootCollection?.GetSafRootForStorable(parentStorable);
            if (safRoot is null)
                return null;

            if (safRoot.StorageRoot.Options.IsReadOnly)
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

            var safRoot = _rootCollection?.GetSafRootForStorable(file);
            if (safRoot is null)
                return null;

            var fileAccess = ToFileAccess(mode);
            if (safRoot.StorageRoot.Options.IsReadOnly && fileAccess.HasFlag(FileAccess.Write))
                return null;

            var stream = childFile.TryOpenStreamAsync(fileAccess).ConfigureAwait(false).GetAwaiter().GetResult();
            if (stream is null)
                return null;

            var parcelFileMode = ToParcelFileMode(mode);
            if (safRoot.StorageRoot.Options.IsReadOnly && parcelFileMode is ParcelFileMode.WriteOnly or ParcelFileMode.ReadWrite)
                return null;

            return _storageManager.OpenProxyFileDescriptor(parcelFileMode, new ReadWriteCallbacks(stream), new Handler(Looper.MainLooper));

            // var storageManager = (StorageManager?)this.Context?.GetSystemService(Context.StorageService);
            // if (storageManager is null)
            //     return null;
            //
            // var parcelFileMode = ToParcelFileMode(mode);
            // return storageManager.OpenProxyFileDescriptor(parcelFileMode, new ReadWriteCallbacks(stream), new Handler(Looper.MainLooper!));

            static ParcelFileMode ToParcelFileMode(string? fileMode)
            {
                return fileMode switch
                {
                    "r" => ParcelFileMode.ReadOnly,
                    "w" => ParcelFileMode.WriteOnly,
                    "rw" => ParcelFileMode.ReadWrite,
                    _ => throw new ArgumentException($"Unsupported mode: {fileMode}.")
                };
            }

            static FileAccess ToFileAccess(string? fileMode)
            {
                return fileMode switch
                {
                    "r" => FileAccess.Read,
                    "w" => FileAccess.Write,
                    "rw" => FileAccess.ReadWrite,
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
            if (storable is null)
                return matrix;

            AddDocumentAsync(matrix, storable, documentId).ConfigureAwait(false).GetAwaiter().GetResult();
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
                AddDocumentAsync(matrix, item, null).ConfigureAwait(false).GetAwaiter().GetResult();
            }

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
                return;

            var safRoot = _rootCollection?.GetSafRootForStorable(storable);
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

            var safRoot = _rootCollection?.GetSafRootForStorable(destinationStorable);
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
                    var movedFile = destinationFolder.MoveFromAsync(file, sourceParentFolder, false, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    return Path.Combine(targetParentDocumentId, movedFile.Name);
                }

                case IChildFolder folder:
                {
                    var movedFolder = destinationFolder.MoveFromAsync(folder, sourceParentFolder, false, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    return Path.Combine(targetParentDocumentId, movedFolder.Name);
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
                return null;

            var safRoot = _rootCollection?.GetSafRootForStorable(storable);
            if (safRoot is null)
                return null;

            if (safRoot.StorageRoot.Options.IsReadOnly)
                return null;

            var parentFolder = storableChild.GetParentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (parentFolder is not IRenamableFolder renamableFolder)
                return null;

            var renamedItem = renamableFolder.RenameAsync(storableChild, displayName).ConfigureAwait(false).GetAwaiter().GetResult();
            if (renamedItem is IWrapper<IFile> { Inner: IWrapper<global::Android.Net.Uri> fileUriWrapper })
                return fileUriWrapper.Inner.ToString();

            if (renamedItem is IWrapper<IFolder> { Inner: IWrapper<global::Android.Net.Uri> folderUriWrapper })
                return folderUriWrapper.Inner.ToString();

            throw new InvalidOperationException($"{nameof(renamedItem)} does not implement {nameof(IWrapper<global::Android.Net.Uri>)}.");
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

            var safRoot = _rootCollection?.GetSafRootForStorable(destinationStorable);
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
                    return Path.Combine(targetParentDocumentId, copiedFile.Name);
                }

                case IFolder folder:
                {
                    var copiedFolder = destinationFolder.CreateCopyOfAsync(folder, false).ConfigureAwait(false).GetAwaiter().GetResult();
                    return Path.Combine(targetParentDocumentId, copiedFolder.Name);
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
                using var inputStream = file.OpenReadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                using var thumbnailStream = ThumbnailHelpers.GenerateImageThumbnailAsync(inputStream, 300U).ConfigureAwait(false).GetAwaiter().GetResult();

                // Need to copy thumbnail stream to a pipe (ParcelFileDescriptor with input/output stream)
                var twoWayPipe = ParcelFileDescriptor.CreatePipe();
                if (twoWayPipe is null)
                    return null;

                var output = new ParcelFileDescriptor.AutoCloseOutputStream(twoWayPipe[1]);
                Task.Run(() =>
                {
                    try
                    {
                        int bytesRead;
                        var buffer = new byte[8192];
                        while ((bytesRead = thumbnailStream.Read(buffer, 0, buffer.Length)) > 0)
                            output.Write(buffer, 0, bytesRead);

                        output.Flush();
                        output.Close();
                    }
                    catch
                    {
                        // Handle if needed
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
