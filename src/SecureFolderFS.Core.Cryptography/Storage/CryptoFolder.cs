using SecureFolderFS.Core.FileSystem.Helpers;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    //public class CryptoFolder : CryptoStorable<IFolder>, IFolderExtended, IModifiableFolder, INestedFolder, IWrappable<IFolder>
    //{
    //    public CryptoFolder(IFolder inner)
    //        : base(inner)
    //    {
    //    }

    //    #region IModifiableFolder

    //    /// <inheritdoc/>
    //    public virtual Task DeleteAsync(INestedStorable item, bool permanently = default, CancellationToken cancellationToken = default)
    //    {
    //        if (Inner is not IModifiableFolder modifiableFolder)
    //            throw new NotSupportedException("Modifying folder contents is not supported.");

    //        return modifiableFolder.DeleteAsync(item, permanently, cancellationToken);
    //    }

    //    /// <inheritdoc/>
    //    public virtual async Task<INestedFile> CreateFileAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
    //    {
    //        if (Inner is not IModifiableFolder modifiableFolder)
    //            throw new NotSupportedException("Modifying folder contents is not supported.");

    //        var encryptedName = EncryptName(desiredName);
    //        var file = await modifiableFolder.CreateFileAsync(encryptedName, overwrite, cancellationToken);

    //        return (INestedFile)Wrap(file);
    //    }

    //    /// <inheritdoc/>
    //    public virtual async Task<INestedFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
    //    {
    //        if (Inner is not IModifiableFolder modifiableFolder)
    //            throw new NotSupportedException("Modifying folder contents is not supported.");

    //        var encryptedName = EncryptName(desiredName);
    //        var folder = await modifiableFolder.CreateFolderAsync(encryptedName, overwrite, cancellationToken);
    //        if (folder is not IModifiableFolder createdModifiableFolder)
    //            throw new ArgumentException("The created folder is not modifiable.");

    //        // Get the DirectoryID file
    //        var dirIdFile = await createdModifiableFolder.CreateFileAsync(FileSystem.Constants.DIRECTORY_ID_FILENAME, false, cancellationToken);
    //        var directoryId = Guid.NewGuid().ToByteArray();

    //        // Initialize directory with DirectoryID
    //        await using var directoryIdStream = await dirIdFile.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
    //        await directoryIdStream.WriteAsync(directoryId, cancellationToken);

    //        // Set DirectoryID to known IDs
    //        directoryIdCache.SetDirectoryId(dirIdFile.Id, Guid.NewGuid().ToByteArray());

    //        return (INestedFolder)Wrap(folder);
    //    }

    //    #endregion

    //    /// <inheritdoc/>
    //    public virtual async IAsyncEnumerable<INestedStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    //    {
    //        await foreach (var item in Inner.GetItemsAsync(kind, cancellationToken))
    //        {
    //            if (PathHelpers.IsCoreFile(item.Name))
    //                continue;

    //            yield return item switch
    //            {
    //                IFile file => (INestedStorable)Wrap(file),
    //                IFolder folder => (INestedStorable)Wrap(folder),
    //                _ => throw new InvalidOperationException("The enumerated item was neither a file nor a folder.")
    //            };
    //        }
    //    }

    //    /// <inheritdoc/>
    //    public virtual async Task<INestedFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
    //    {
    //        if (Inner is not IFolderExtended folderExtended)
    //            throw new NotSupportedException("Retrieving individual files is not supported.");

    //        var encryptedName = EncryptName(fileName);
    //        var file = await folderExtended.GetFileAsync(encryptedName, cancellationToken);

    //        return (INestedFile)Wrap(file);
    //    }

    //    /// <inheritdoc/>
    //    public virtual async Task<INestedFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
    //    {
    //        if (Inner is not IFolderExtended folderExtended)
    //            throw new NotSupportedException("Retrieving individual folders is not supported.");

    //        var encryptedName = EncryptName(folderName);
    //        var folder = await folderExtended.GetFolderAsync(encryptedName, cancellationToken);

    //        return (INestedFolder)Wrap(folder);
    //    }
    //}
}
