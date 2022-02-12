using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.StorageEnumeration;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FindFilesWithPatternCallback : BaseDokanOperationsCallbackWithPath, IFindFilesWithPatternCallback
    {
        private readonly IContentCryptor _contentCryptor;

        private readonly IStorageEnumerator _storageEnumerator;

        public FindFilesWithPatternCallback(IContentCryptor contentCryptor, IStorageEnumerator storageEnumerator, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._contentCryptor = contentCryptor;
            this._storageEnumerator = storageEnumerator;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            try
            {
                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                files = _storageEnumerator
                    .EnumerateFileSystemEntries(ciphertextPath.Path, searchPattern)
                    .Select<FileEnumerationInfo, FileInformation?>(item =>
                    {
                        if (PathHelpers.IsCoreFile(item.FileName))
                        {
                            return null;
                        }

                        try
                        {
                            var cleartextFileName = pathReceiver.GetCleartextFileName(Path.Combine(ciphertextPath.Path, item.FileName));

                            return new FileInformation()
                            {
                                FileName = cleartextFileName,
                                Attributes = item.Attributes,
                                CreationTime = item.CreationTime,
                                LastAccessTime = item.LastAccessTime,
                                LastWriteTime = item.LastWriteTime,
                                Length = item.IsFile ? _contentCryptor.FileContentCryptor.CalculateCleartextSize(item.Length - _contentCryptor.FileHeaderCryptor.HeaderSize) : item.Length
                            };
                        }
                        catch (CryptographicException)
                        {
                            return null;
                        }
                        catch (FormatException)
                        {
                            return null;
                        }
                    })
                    .Where(item => item != null)
                    .Select(item => (FileInformation)item!)
                    .ToArray();

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                files = Array.Empty<FileInformation>();
                return DokanResult.InvalidName;
            }
        }
    }
}
