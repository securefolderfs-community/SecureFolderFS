using DokanNet;
using System;
using System.IO;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class MoveFileCallback : BaseDokanOperationsCallbackWithPath, IMoveFileCallback
    {
        public MoveFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            var oldCiphertextPath = GetCiphertextPath(oldName);
            var newCiphertextPath = GetCiphertextPath(newName);

            CloseHandle(info);
            InvalidateContext(info);

            var newPathExists = info.IsDirectory ? Directory.Exists(newCiphertextPath) : File.Exists(newCiphertextPath);

            try
            {
                if (!newPathExists)
                {
                    if (info.IsDirectory)
                    {
                        Directory.Move(oldCiphertextPath, newCiphertextPath);
                    }
                    else
                    {
                        File.Move(oldCiphertextPath, newCiphertextPath);
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

                    // File
                    File.Delete(newCiphertextPath);
                    File.Move(oldCiphertextPath, newCiphertextPath);
                    
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

                return DokanResult.AlreadyExists;
            }
        }
    }
}
