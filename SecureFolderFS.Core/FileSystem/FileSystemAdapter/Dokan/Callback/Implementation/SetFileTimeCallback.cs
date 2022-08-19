using DokanNet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Sdk.Paths;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class SetFileTimeCallback : BaseDokanOperationsCallbackWithPath, ISetFileTimeCallback
    {
        private readonly IFileSystemOperations _fileSystemOperations;

        public SetFileTimeCallback(IFileSystemOperations fileSystemOperations, VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
            _fileSystemOperations = fileSystemOperations;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            try
            {
                if (!IsContextInvalid(info))
                {
                    var ct = creationTime?.ToFileTime() ?? 0L;
                    var lat = lastAccessTime?.ToFileTime() ?? 0L;
                    var lwt = lastWriteTime?.ToFileTime() ?? 0L;

                    if (handles.GetHandle(GetContextValue(info)) is FileHandle fileHandle)
                    {
                        if (fileHandle.SetFileTime(ref ct, ref lat, ref lwt))
                        {
                            return DokanResult.Success;
                        }
                    }
                    else
                    {
                        return DokanResult.InvalidHandle;
                    }

                    var hrException = Marshal.GetExceptionForHR(Marshal.GetLastWin32Error());
                    if (hrException is not null)
                    {
                        throw hrException;
                    }
                }

                ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);

                if (creationTime is not null)
                    _fileSystemOperations.DangerousFileOperations.SetCreationTime(ciphertextPath.Path, creationTime.Value);

                if (lastAccessTime is not null)
                    _fileSystemOperations.DangerousFileOperations.SetLastAccessTime(ciphertextPath.Path, lastAccessTime.Value);

                if (lastWriteTime is not null)
                    _fileSystemOperations.DangerousFileOperations.SetLastWriteTime(ciphertextPath.Path, lastWriteTime.Value);

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
        }
    }
}
