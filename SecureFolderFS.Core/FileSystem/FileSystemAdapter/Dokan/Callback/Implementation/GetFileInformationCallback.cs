using DokanNet;
using System;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class GetFileInformationCallback : BaseDokanOperationsCallbackWithPath, IGetFileInformationCallback
    {
        private readonly IContentCryptor _contentCryptor;

        public GetFileInformationCallback(IContentCryptor contentCryptor, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _contentCryptor = contentCryptor;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            try
            {
                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                FileSystemInfo finfo = new FileInfo(ciphertextPath.Path);
                if (!finfo.Exists)
                    finfo = new DirectoryInfo(ciphertextPath.Path);
                
                fileInfo = new FileInformation()
                {
                    FileName = finfo.Name,
                    Attributes = finfo.Attributes,
                    CreationTime = finfo.CreationTime,
                    LastAccessTime = finfo.LastAccessTime,
                    LastWriteTime = finfo.LastWriteTime,
                    Length = finfo is FileInfo fileInfo2
                        ? _contentCryptor.FileContentCryptor.CalculateCleartextSize(fileInfo2.Length - _contentCryptor.FileHeaderCryptor.HeaderSize)
                        : 0L
                };

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                fileInfo = default;
                return DokanResult.InvalidName;
            }
            catch (FileNotFoundException)
            {
                fileInfo = default;
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                fileInfo = default;
                return DokanResult.PathNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                fileInfo = default;
                return DokanResult.AccessDenied;
            }
            catch (Exception)
            {
                fileInfo = default;
                return DokanResult.Unsuccessful;
            }
        }
    }
}
