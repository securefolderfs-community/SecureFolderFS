using DokanNet;
using System;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetFileAttributesCallback : BaseDokanOperationsCallbackWithPath, ISetFileAttributesCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public SetFileAttributesCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._fileSystemOperations = fileSystemOperations;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            try
            {
                // MS-FSCC 2.6 File Attributes : There is no file attribute with the value 0x00000000
                // because a value of 0x00000000 in the FileAttributes field means that the file attributes for this file MUST NOT be changed when setting basic information for the file
                if (attributes != 0)
                {
                    ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);
                    _fileSystemOperations.DangerousFileOperations.SetAttributes(ciphertextPath.Path, attributes);
                }

                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                return DokanResult.InvalidName;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanResult.PathNotFound;
            }
        }
    }
}
