using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Storage.Extensions
{
    public static partial class StorageExtensions
    {
        /// <inheritdoc cref="CreateCopyOfStorableAsync{TStorable}(IModifiableFolder,TStorable,bool,IProgress{IStorable},CancellationToken)"/>
        public static Task<TStorable> CreateCopyOfStorableAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToCopy, bool overwrite = false, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            return CreateCopyOfStorableAsync(destinationFolder, itemToCopy, overwrite, null, cancellationToken);
        }

        /// <summary>
        /// Creates a copy of the provided storable within this folder.
        /// </summary>
        /// <typeparam name="TStorable">The type of storable, whether a <see cref="IFile"/> or a <see cref="IFolder"/>.</typeparam>
        /// <param name="destinationFolder">The folder where the copy is created.</param>
        /// <param name="itemToCopy">The storable to be copied into this folder.</param>
        /// <param name="overwrite"><code>true</code> if any existing destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<TStorable> CreateCopyOfStorableAsync<TStorable>(this IModifiableFolder destinationFolder, TStorable itemToCopy, bool overwrite, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            return itemToCopy switch
            {
                IFile fileToCopy => (TStorable)await destinationFolder.CreateCopyOfAsync(fileToCopy, overwrite, cancellationToken),
                IFolder folderToCopy => (TStorable)await destinationFolder.CreateCopyOfAsync(folderToCopy, overwrite, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(itemToCopy))
            };
        }

        /// <inheritdoc cref="CreateCopyOfAsync(IModifiableFolder,IFolder,bool,IProgress{IStorable},CancellationToken)"/>
        public static Task<IModifiableFolder> CreateCopyOfAsync(this IModifiableFolder destinationFolder, IFolder folderToCopy, bool overwrite, CancellationToken cancellationToken = default)
        {
            return CreateCopyOfAsync(destinationFolder, folderToCopy, overwrite, null, cancellationToken);
        }

        /// <summary>
        /// Creates a copy of the provided folder within this folder.
        /// </summary>
        /// <param name="destinationFolder">The folder where the copy is created.</param>
        /// <param name="folderToCopy">The folder to be copied into this folder.</param>
        /// <param name="overwrite"><code>true</code> if any existing destination folder can be overwritten; otherwise, <c>false</c>.</param>
        /// <param name="reporter">An optional <see cref="IProgress{T}"/> instance where all progress notifications are forwarded to.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        public static async Task<IModifiableFolder> CreateCopyOfAsync(this IModifiableFolder destinationFolder, IFolder folderToCopy, bool overwrite, IProgress<IStorable>? reporter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Support fast-path

            // Create the corresponding folder in the destination
            var copiedFolder = (IModifiableFolder)await destinationFolder.CreateFolderAsync(folderToCopy.Name, overwrite, cancellationToken);
            reporter?.Report(copiedFolder);

            // Iterate through all items in the source folder
            await foreach (var item in folderToCopy.GetItemsAsync(StorableType.All, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (item)
                {
                    case IFile file:
                    {
                        // Copy the file to the destination folder
                        var copiedFile = await copiedFolder.CreateCopyOfAsync(file, overwrite, cancellationToken);
                        reporter?.Report(copiedFile);

                        break;
                    }

                    case IFolder subFolder:
                    {
                        // Recursively copy the subfolder
                        await copiedFolder.CreateCopyOfAsync(subFolder, overwrite, reporter, cancellationToken);
                        break;
                    }
                }
            }

            return copiedFolder;
        }
    }
}
