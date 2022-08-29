using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Security;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class GetFileInformationCallback : BaseDokanOperationsCallbackWithPath, IGetFileInformationCallback
    {
        private readonly ISecurity _security;

        public GetFileInformationCallback(ISecurity security, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _security = security;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            try
            {
                var ciphertextPath = GetCiphertextPath(fileName);

                FileSystemInfo finfo = new FileInfo(ciphertextPath);
                if (!finfo.Exists)
                    finfo = new DirectoryInfo(ciphertextPath);
                
                fileInfo = new FileInformation()
                {
                    FileName = finfo.Name,
                    Attributes = finfo.Attributes,
                    CreationTime = finfo.CreationTime,
                    LastAccessTime = finfo.LastAccessTime,
                    LastWriteTime = finfo.LastWriteTime,
                    Length = finfo is FileInfo fileInfo2
                        ? _security.ContentCrypt.CalculateCleartextSize(fileInfo2.Length - _security.HeaderCrypt.HeaderCiphertextSize)
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
