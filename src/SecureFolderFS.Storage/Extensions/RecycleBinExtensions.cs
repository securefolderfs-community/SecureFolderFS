using OwlCore.Storage;
using SecureFolderFS.Storage.Pickers;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Storage.Extensions
{
    public static class RecycleBinExtensions
    {
        public static bool IsRecycleBinEnabled(this FileSystemOptions options)
        {
            return options.RecycleBinSize != 0;
        }

        /// <inheritdoc cref="IRecycleBinFolder.RestoreItemsAsync"/>
        public static async Task<bool> TryRestoreItemsAsync(this IRecycleBinFolder recycleBinFolder, IEnumerable<IStorableChild> items, IFolderPicker folderPicker, CancellationToken cancellationToken = default)
        {
            try
            {
                await recycleBinFolder.RestoreItemsAsync(items, folderPicker, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
