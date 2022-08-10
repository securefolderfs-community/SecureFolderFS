using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using SecureFolderFS.Sdk.Storage.Enums;

namespace SecureFolderFS.Sdk.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        public static async IAsyncEnumerable<IFile> GetFilesAsync(this IFolder folder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in folder.GetItemsAsync(StorableKind.Files, cancellationToken))
            {
                if (item is IFile fileItem)
                    yield return fileItem;
            }
        }

        public static async IAsyncEnumerable<IFolder> GetFoldersAsync(this IFolder folder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var item in folder.GetItemsAsync(StorableKind.Files, cancellationToken))
            {
                if (item is IFolder folderItem)
                    yield return folderItem;
            }
        }
    }
}
