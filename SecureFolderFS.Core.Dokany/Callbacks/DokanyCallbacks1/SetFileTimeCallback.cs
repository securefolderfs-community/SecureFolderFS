using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Paths;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.Dokany.Callbacks.DokanyCallbacks
{
    internal sealed class SetFileTimeCallback : BaseDokanOperationsCallbackWithPath, ISetFileTimeCallback
    {
        public SetFileTimeCallback(VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(vaultPath, pathConverter, handles)
        {
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

                    if (handles.GetHandle<FileHandle>(GetContextValue(info)) is { } fileHandle)
                    {
                        // TODO: Re-add set file time
                        //if (fileHandle.SetFileTime(ref ct, ref lat, ref lwt))
                        //{
                            return DokanResult.Success;
                        //}
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

                var ciphertextPath = GetCiphertextPath(fileName);

                if (creationTime is not null)
                    File.SetCreationTime(ciphertextPath, creationTime.Value);

                if (lastAccessTime is not null)
                    File.SetLastAccessTime(ciphertextPath, lastAccessTime.Value);

                if (lastWriteTime is not null)
                    File.SetLastWriteTime(ciphertextPath, lastWriteTime.Value);

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
