using DokanNet;
using System;
using System.IO;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class MoveFileCallback : BaseDokanOperationsCallbackWithPath, IMoveFileCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public MoveFileCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            this._fileSystemOperations = fileSystemOperations;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            ConstructFilePath(oldName, out ICiphertextPath oldCiphertextPath);
            ConstructFilePath(newName, out ICiphertextPath newCiphertextPath);

            handles.Close(GetContextValue(info));
            InvalidateContext(info);

            var newPathExists = info.IsDirectory
                ? _fileSystemOperations.DangerousDirectoryOperations.Exists(newCiphertextPath.Path)
                : _fileSystemOperations.DangerousFileOperations.Exists(newCiphertextPath.Path);

            try
            {
                if (!newPathExists)
                {
                    if (info.IsDirectory)
                    {
                        _fileSystemOperations.MoveDirectory(oldCiphertextPath, newCiphertextPath);
                    }
                    else
                    {
                        _fileSystemOperations.MoveFile(oldCiphertextPath, newCiphertextPath);
                    }

                    return DokanResult.Success;
                }
                else if (replace)
                {
                    if (info.IsDirectory)
                    {
                        // Cannot replace directory destination - See MOVEFILE_REPLACE_EXISTING
                        return DokanResult.AccessDenied;
                    }
                    else
                    {
                        if (_fileSystemOperations.PrepareFileForDeletion(newCiphertextPath))
                        {
                            _fileSystemOperations.DangerousFileOperations.DeleteFile(newCiphertextPath.Path);
                            _fileSystemOperations.MoveFile(oldCiphertextPath, newCiphertextPath);
                        }
                    }

                    return DokanResult.Success;
                }
                else
                {
                    return DokanResult.FileExists;
                }
            }
            catch (PathTooLongException)
            {
                return DokanResult.InvalidName;
            }
            catch (FileNotFoundException)
            {
                return DokanResult.FileNotFound;
            }
            catch (DirectoryNotFoundException)
            {
                return DokanResult.PathNotFound;
            }
            catch (UnauthorizedAccessException)
            {
                return DokanResult.AccessDenied;
            }
            catch (IOException ioex)
            {
                if (ioex.IsFileAlreadyExistsException())
                {
                    // File already exists
                    return DokanResult.AlreadyExists;
                }
                else if (ioex.IsDirectoryNotEmptyException())
                {
                    // Directory not empty
                    return DokanResult.DirectoryNotEmpty;
                }
                else if (ioex.IsDiskFullException())
                {
                    // Disk full
                    return DokanResult.DiskFull;
                }

                return DokanResult.Unsuccessful;
            }
        }
    }
}
