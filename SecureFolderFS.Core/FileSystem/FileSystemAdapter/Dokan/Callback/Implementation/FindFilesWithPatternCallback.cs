using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FindFilesWithPatternCallback : BaseDokanOperationsCallbackWithPath, IFindFilesWithPatternCallback
    {
        private readonly IContentCryptor _contentCryptor;

        public FindFilesWithPatternCallback(IContentCryptor contentCryptor, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _contentCryptor = contentCryptor;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            try
            {
                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                files = new DirectoryInfo(ciphertextPath.Path)
                    .EnumerateFileSystemInfos()
                    .Select<FileSystemInfo, FileInformation?>(finfo =>
                    {
                        if (PathHelpers.IsCoreFile(finfo.Name) || !DokanHelper.DokanIsNameInExpression(searchPattern, finfo.Name, true))
                            return null;

                        try
                        {
                            var cleartextFileName = pathReceiver.GetCleartextFileName(finfo.FullName);

                            return new FileInformation()
                            {
                                FileName = cleartextFileName,
                                Attributes = finfo.Attributes,
                                CreationTime = finfo.CreationTime,
                                LastAccessTime = finfo.LastAccessTime,
                                LastWriteTime = finfo.LastWriteTime,
                                Length = finfo is FileInfo fileInfo
                                    ? _contentCryptor.FileContentCryptor.CalculateCleartextSize(fileInfo.Length - _contentCryptor.FileHeaderCryptor.HeaderSize)
                                    : 0L
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
                    .Where(x => x is not null)
                    .Select(x => (FileInformation)x!)
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
