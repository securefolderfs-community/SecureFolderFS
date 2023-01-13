using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FindFilesWithPatternCallback : BaseDokanOperationsCallbackWithPath, IFindFilesWithPatternCallback
    {
        private readonly ISecurity _security;

        public FindFilesWithPatternCallback(ISecurity security, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _security = security;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            try
            {
                files = new DirectoryInfo(GetCiphertextPath(fileName))
                    .EnumerateFileSystemInfos()
                    .Select<FileSystemInfo, FileInformation?>(finfo =>
                    {
                        if (PathHelpers.IsCoreFile(finfo.Name))
                            return null;

                        try
                        {
                            var cleartextFileName = pathReceiver.GetCleartextFileName(finfo.FullName);
                            if (!DokanHelper.DokanIsNameInExpression(searchPattern, cleartextFileName, false))
                                return null;

                            return new FileInformation()
                            {
                                FileName = cleartextFileName,
                                Attributes = finfo.Attributes,
                                CreationTime = finfo.CreationTime,
                                LastAccessTime = finfo.LastAccessTime,
                                LastWriteTime = finfo.LastWriteTime,
                                Length = finfo is FileInfo fileInfo
                                    ? _security.ContentCrypt.CalculateCleartextSize(fileInfo.Length - _security.HeaderCrypt.HeaderCiphertextSize)
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
