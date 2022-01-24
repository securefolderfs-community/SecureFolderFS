using DokanNet;
using System;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.StorageEnumeration;
using SecureFolderFS.Core.Security.ContentCrypt;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class GetFileInformationCallback : BaseDokanOperationsCallbackWithPath, IGetFileInformationCallback
    {
        private readonly IContentCryptor _contentCryptor;

        private readonly IStorageEnumerator _storageEnumerator;

        public GetFileInformationCallback(IContentCryptor contentCryptor, IStorageEnumerator storageEnumerator, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._contentCryptor = contentCryptor;
            this._storageEnumerator = storageEnumerator;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            try
            {
                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);
                FileEnumerationInfo fileEnumerationInfo = _storageEnumerator.GetFileInfo(ciphertextPath.Path);

                fileInfo = new FileInformation()
                {
                    FileName = fileEnumerationInfo.FileName,
                    Attributes = fileEnumerationInfo.Attributes,
                    CreationTime = fileEnumerationInfo.CreationTime,
                    LastAccessTime = fileEnumerationInfo.LastAccessTime,
                    LastWriteTime = fileEnumerationInfo.LastWriteTime,
                    Length = fileEnumerationInfo.IsFile
                        ? _contentCryptor.FileContentCryptor.CalculateCleartextSize(fileEnumerationInfo.Length - _contentCryptor.FileHeaderCryptor.HeaderSize)
                        : fileEnumerationInfo.Length
                };

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                fileInfo = new FileInformation();
                return DokanResult.InvalidName;
            }
            catch (FileNotFoundException)
            {
                fileInfo = new FileInformation();
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                fileInfo = new FileInformation();
                return DokanResult.PathNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                fileInfo = new FileInformation();
                return DokanResult.AccessDenied;
            }
            catch (Exception)
            {
                fileInfo = new FileInformation();
                return DokanResult.Unsuccessful;
            }
        }
    }
}
