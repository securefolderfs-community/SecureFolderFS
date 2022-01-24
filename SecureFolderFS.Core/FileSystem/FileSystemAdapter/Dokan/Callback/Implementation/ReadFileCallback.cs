using DokanNet;
using System;
using System.Diagnostics;
using System.IO;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Exceptions;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class ReadFileCallback : BaseDokanOperationsCallbackWithPath, IReadFileCallback
    {
        public ReadFileCallback(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(vaultPath, pathReceiver, handles)
        {
        }

        public NtStatus ReadFile(string fileName, IntPtr buffer, uint bufferLength, out int bytesRead, long offset, IDokanFileInfo info)
        {
            if (info.IsDirectory)
            {
                bytesRead = 0;
                return DokanResult.AccessDenied;
            }

            ConstructFilePath(fileName, out ICiphertextPath ciphertextPath);
            long contextHandle = GetContextValue(info);
            bool opened = false;

            try
            {
                if (handles.GetHandle(contextHandle) is not FileHandle fileHandle)
                {
                    // Invalid handle...
                    contextHandle = handles.OpenHandleToFile(ciphertextPath, FileMode.Open, System.IO.FileAccess.Read,
                        FileShare.Read, FileOptions.None);
                    fileHandle = (FileHandle)handles.GetHandle(contextHandle);

                    opened = true;
                }

                // Read file
                bytesRead = StreamHelpers.ReadToIntPtrBuffer(fileHandle.CleartextFileStream, buffer, bufferLength,
                    offset);
                return DokanResult.Success;
            }
            catch (PathTooLongException)
            {
                bytesRead = 0;
                return DokanResult.InvalidName;
            }
            catch (IntegrityException)
            {
                bytesRead = 0;
                return NtStatus.CrcError;
            }
            catch (UnavailableStreamException)
            {
                bytesRead = 0;
                return NtStatus.HandleNoLongerValid;
            }
            catch (Exception ex)
            {
                _ = ex;
                bytesRead = 0;

                Debugger.Break();
                return DokanResult.InternalError;
            }
            finally
            {
                if (opened)
                {
                    handles.Close(contextHandle);
                }
            }
        }
    }
}
